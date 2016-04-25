using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb_core
{
	public class LCDController : ClockedComponent
	{
		public static class Registers
		{
			// LCD display registers
			public const ushort LCDC = 0xFF40; // LCD control
			public const ushort STAT = 0xFF41; // LCD status
			public const ushort SCY = 0xFF42; // scroll y
			public const ushort SCX = 0xFF43; // scroll x
			public const ushort LY = 0xFF44; // LCD y co-ord
			public const ushort LYC = 0xFF45; // LCD y compare

			// DMA register
			public const ushort DMA = 0xFF46;

			// palette data
			public const ushort BGP = 0xFF47; // background palette
			public const ushort OBP0 = 0xFF48; // object (sprite) palette 0
			public const ushort OBP1 = 0xFF49; // object (sprite) palette 1

			// LCD display registers
			public const ushort WY = 0xFF4A; // window y
			public const ushort WX = 0xFF4B; // window x
		}

		public enum LcdMode : byte
		{
			HBlank = 0x00,
			VBlank = 0x01,
			OAMRead = 0x02,
			VRAMRead = 0x03,
		}

		private const int ScreenWidth = 160;
		private const int ScreenHeight = 144;

		private byte[] _screenData;

		private byte[] _vram;
		private byte[] _oam;

		private byte _lcdControl;					// LCDC value store
		private bool _displayEnabled;				// LCDC bit 7 - 0: Off / 1: On
		private ushort _windowTileBaseAddress;		// LCDC bit 6 - 0: 0x9800 - 0x9BFF / 1 : 0x9C00 = 0x9FFF
		private bool _windowEnabled;				// LCDC bit 5 - 0: Off / 1: On
		private ushort _backgroundCharBaseAddress;	// LCDC bit 4 - 0: 0x8800 - 0x97FF / 1 : 0x8000 = 0x8FFF
		private ushort _backgroundTileBaseAddress;	// LCDC bit 3 - 0: 0x9800 - 0x9BFF / 1 : 0x9C00 = 0x9FFF
		private int _spriteHeight;					// LCDC bit 2 - 0: 8x8 / 1: 8x16 (w*h)
		private bool _spritesEnabled;				// LCDC bit 1 - 0: Off / 1: On
		private bool _backgroundEnabled;			// LCDC bit 0 - 0: Off / 1: On

		private bool _signedCharIdentifier;			// if the background char data is in the range 0x8800 - 0x97FF then tile identifiers are signed

		private byte _lcdStatus;					// STAT value store
		private LcdMode _lcdMode;					// STAT bit 0 - 1: LCD mode flag

		private byte _scrollY, _scrollX;			// SCY, SCX

		private byte _currentScanline;				// LY
		private byte _scanlineCompare;				// LYC

		// palette data
		private byte _bgp;							// BGP value store
		private byte[] _backgroundPalette;			// background palette data

		private byte _obp0, _obp1;					// OBPn value store
		private byte[][] _spritePalette;			// object (sprite) palette data

		private byte _windowY, _windowX;			// WY, WX

		private uint _frameClock;					// counter of clock cycles elapsed in the current frame
		private uint _vblankClock;					// counter of clock cycles elapsed in current scanline in vblank

		public LCDController(GameBoy gameBoy)
			: base(gameBoy)
		{
			_screenData = new byte[ScreenWidth * ScreenHeight];

			_vram = new byte[0x2000];
			_oam = new byte[0xA0];

			_backgroundPalette = new byte[4];

			_spritePalette = new byte[2][];
			_spritePalette[0] = new byte[4];
			_spritePalette[1] = new byte[4];
		}

		public byte ReadByte(ushort address)
		{
			SynchroniseWithSystemClock();

			switch (address)
			{
				case Registers.LCDC:
					return _lcdControl;

				case Registers.STAT:
					return (byte)((_lcdStatus & ~0x03) | (byte)_lcdMode);

				case Registers.SCY:
					return _scrollY;

				case Registers.SCX:
					return _scrollX;

				case Registers.LY:
					return _currentScanline;

				case Registers.LYC:
					return _scanlineCompare;

				case Registers.BGP:
					return _bgp;

				case Registers.OBP0:
					return _obp0;

				case Registers.OBP1:
					return _obp1;

				case Registers.WY:
					return _windowY;

				case Registers.WX:
					return _windowX;
			}

			throw new ArgumentOutOfRangeException("address");
		}

		public void WriteByte(ushort address, byte value)
		{
			SynchroniseWithSystemClock();

			if (address >= 0x8000 && address <= 0x9fff)
			{
				_vram[address & 0x1FFF] = value;
			}
			else if (address >= 0xFE00 && address <= 0xFE9F)
			{
				_oam[address & 0xFF] = value;
			}
			else
			{
				switch (address)
				{
					case Registers.LCDC:
						_lcdControl = value;

						_displayEnabled = (_lcdControl & 0x80) == 0x80;

						// lcd starts in mode 2 when enabled
						if (_displayEnabled)
						{
							_lcdMode = LcdMode.OAMRead;
						}
						// reset LY when display is disabled
						else
						{
							_currentScanline = 0;
							CompareScanlineValue();

							_frameClock = 0;
						}

						_windowTileBaseAddress = (ushort)((_lcdControl & 0x40) == 0x40 ? 0x9C00 : 0x9800);
						_windowEnabled = (_lcdControl & 0x20) == 0x20;
						_backgroundCharBaseAddress = (ushort)((_lcdControl & 0x10) == 0x10 ? 0x8000 : 0x8800);

						// if the background char data is in the range 0x8800 - 0x97FF then tile identifiers are signed
						_signedCharIdentifier = _backgroundCharBaseAddress == 0x8800;

						_backgroundTileBaseAddress = (ushort)((_lcdControl & 0x08) == 0x08 ? 0x9C00 : 0x9800);
						_spriteHeight = (_lcdControl & 0x04) == 0x04 ? 16 : 8;
						_spritesEnabled = (_lcdControl & 0x02) == 0x02;
						_backgroundEnabled = (_lcdControl & 0x01) == 0x01;

						break;

					case Registers.STAT:
						// capture lcd interrupt selection from value
						// interrupt selection stored in bits 6-3, preserve bits 0-2
						// and mask bits 6-3 from value passed in
						_lcdStatus = (byte)((_lcdStatus & 0x07) | (value & 0x78));

						// a write to the LY = LYC match flag (bit 2) clears the flag
						if ((value & 0x04) == 0x04)
						{
							unchecked { _lcdStatus &= (byte)~0x04; }
						}

						break;

					case Registers.SCY:
						_scrollY = value;
						break;

					case Registers.SCX:
						_scrollX = value;
						break;

					case Registers.LYC:
						_scanlineCompare = value;
						break;

					case Registers.DMA:
						// TODO(david): this should take 671 cycles (160 microseconds), does it make a difference?
						int dmaStartAddress = value << 8;

						for (int i = 0; i < 0xA0; i++)
						{
							_oam[i] = _gb.MMU.ReadByte((ushort)(dmaStartAddress + i));
						}
						break;

					case Registers.BGP:
						_bgp = value;

						_backgroundPalette = ExtractPaletteData(_bgp, 0);
						break;

					case Registers.OBP0:
						_obp0 = value;

						// extract sprite palette data, start from index 1, palette entry 00 is always 00
						_spritePalette[0] = ExtractPaletteData(_obp0, 1);
						break;

					case Registers.OBP1:
						_obp1 = value;

						// extract sprite palette data, start from index 1, palette entry 00 is always 00
						_spritePalette[1] = ExtractPaletteData(_obp1, 1);
						break;

					case Registers.WY:
						_windowY = value;
						break;

					case Registers.WX:
						_windowX = value;
						break;

					default:
						throw new ArgumentOutOfRangeException("address");
				}
			}
		}

		private static byte[] ExtractPaletteData(byte value, int startingindex)
		{
			byte[] palette = new byte[4];

			for (int i = startingindex; i < 4; i++)
			{
				palette[i] = (byte)((value >> i * 2) & 0x03);
			}

			return palette;
		}

		private void CompareScanlineValue()
		{
			// compares the current scanline LY with the LYC compare value
			// set status flag and optionally raise interrupt if they match
			if (_currentScanline == _scanlineCompare)
			{
				// set match flag in lcd status register (bit 2)
				_lcdStatus |= 0x04;

				// if the LYC = LY interrupt selection is set then raise the interrupt
				if ((_lcdStatus & 0x40) == 0x40) _gb.RequestInterrupt(Interrupt.LCDCStatus);
			}
			else
			{
				// clear match flag in lcd status register (bit 2)
				unchecked { _lcdStatus &= (byte)~0x04; }
			}
		}

		public override void Update(uint cycleCount)
		{
			// only run lcd controller update if display is enabled
			if (!_displayEnabled)
				return;

			_frameClock += cycleCount;
			
			// LCD runs for 70224 cpu cycles a frame, 456 cycles a scanline and 154 total scanlines
			// 10 of the scanlines are VBlank and the rest are the active portion of the cycle
			// mode 2, 3 and 0 occur in he first 65664 cycles of a frame (70224 - 4560)
			if (_frameClock < 65664)
			{
				// Mode 2 - OAM read, 77-83 cycles of frame
				if ((_frameClock % 456) < 80)
				{
					if (_lcdMode != LcdMode.OAMRead)
					{
						// We've just entered OAM read so we're at the beginning of the line,
						// increment the current scanline unless we're coming from VBlank, then 
						// don't inrecement because we've just set it to 0
						if (_lcdMode != LcdMode.VBlank)
						{
							_currentScanline++;
							CompareScanlineValue(); 
						}

						_lcdMode = LcdMode.OAMRead;

						// raise OAM stat interrupt if requested
						if ((_lcdStatus & 0x20) == 0x20) _gb.RequestInterrupt(Interrupt.LCDCStatus);
					}
				}
				// Mode 3 - OAM and VRAM read, 162-175 cycles of frame
				else if ((_frameClock % 456) < 252)
				{
					if (_lcdMode != LcdMode.VRAMRead)
					{
						_lcdMode = LcdMode.VRAMRead;
					}
				}
				// Mode 0 - HBlank, 201-207 cycles of frame
				else
				{
					if (_lcdMode != LcdMode.HBlank)
					{
						_lcdMode = LcdMode.HBlank;

						// raise hblank stat interrupt if requested
						if ((_lcdStatus & 0x08) == 0x08) _gb.RequestInterrupt(Interrupt.LCDCStatus);

						RenderScanline();
					}
				}
			}
			// Mode 1 - VBlank
			else
			{
				// are we entering vblank from another mode?
				if (_lcdMode != LcdMode.VBlank)
				{
					_lcdMode = LcdMode.VBlank;

					_currentScanline++;
					CompareScanlineValue();

					// count 10 scanlines in vblank, include any already pushing into vblank timing
					_vblankClock = _frameClock - 65664;

					// raise vblank stat interrupt if requested
					if ((_lcdStatus & 0x10) == 0x10) _gb.RequestInterrupt(Interrupt.LCDCStatus);

					// raise vblank interrupt
					_gb.RequestInterrupt(Interrupt.VBlank);

					//DEBUG_DumpVramTiles();

					// we've just entered vblank so the rendering for the frame is finished
					// present the screen data
					_gb.Interface.VideoRefresh(_screenData);
				}
				// processing vblank 
				else
				{
					_vblankClock += cycleCount;

					if (_vblankClock >= 456)
					{
						_vblankClock -= 456;
						_currentScanline++;

						if (_currentScanline > 153)
						{
							_frameClock -= 70224;
							_currentScanline = 0;
						}

						CompareScanlineValue();
					}
				}
			}
		}

		//private void DEBUG_DumpVramTiles()
		//{
		//	for (int i = 0; i < (144 / 8) * (160 / 8); i++)
		//	{
		//		for (int y = 0; y < 8; y++)
		//		{
		//			byte line1 = _vram[(0x10 * i) + (y * 2)];
		//			byte line2 = _vram[(0x10 * i) + (y * 2) + 1];

		//			for (int x = 0; x < 8; x++)
		//			{
		//				int pixelOffset = 7 - x;

		//				var pixel = ((line2 >> pixelOffset) & 0x01) << 1 | (line1 >> pixelOffset) & 0x01;

		//				//_screenData[(y * 160) + ((i * 8) % 160) + x] = _backgroundPalette[pixel];

		//				_screenData[(i * 8 % 160) + x + (y + i * 8 / 160 * 8) * 160] = _backgroundPalette[pixel];
		//			}
		//		}
		//	}
		//}

		private void RenderScanline()
		{
			if (_backgroundEnabled)
				RenderBackgroundScanline();
		}

		private void RenderBackgroundScanline()
		{
			int renderedScanline = _currentScanline +_scrollY;

			// from which tile row are we rendering?
			int tileRow = (renderedScanline / 8) * 32;

			// draw 160 pixel scanline
			for (int x = 0; x < 160; x++)
			{
				// which tile column are we rendering
				int tileColumn = (x + _scrollX) / 8;

				int tileAddress = _backgroundTileBaseAddress + tileRow + tileColumn;

				byte charIdentifier = 0;

				if (_signedCharIdentifier)
				{
					// move the zero point of the tile identifier as these are signed
					// adding 128 so -128 becomes tile 0x00 and 127 becomes 0xFF
					// this simplifies the address decoding as we can just add onto 
					// the char ram base address
					charIdentifier = (byte)((sbyte)_vram[tileAddress & 0x1FFF] + 128);
				}
				else
				{
					charIdentifier = _vram[tileAddress & 0x1FFF];
				}

				int charDataAddress = 0;

				// which line in the tile?
				int line = (renderedScanline % 8);
				
				// generate address of the appropriate line in the tile
				// 16 bytes per character, 2 bytes per line
				// char ram base address + charIdentifier * 16 + line * 2
				charDataAddress = _backgroundCharBaseAddress + (charIdentifier << 4) + (line << 1);

				// decode character pixel data
				byte charData1 = _vram[(charDataAddress) & 0x1FFF];
				byte charData2 = _vram[(charDataAddress + 1) & 0x1FFF];

				int pixelOffset = 7 - ((x + _scrollX) % 8);

				var pixel = ((charData2 >> pixelOffset) & 0x01) << 1 | (charData1 >> pixelOffset) & 0x01;

				_screenData[(_currentScanline * 160) + x] = _backgroundPalette[pixel];
			}
		}
	}
}
