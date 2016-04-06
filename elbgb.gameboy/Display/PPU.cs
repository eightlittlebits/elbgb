using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb.gameboy.Display
{
	public class PPU : ClockedComponent
	{
		public static class Registers
		{
			// LCD display registers
			public const ushort LCDC = 0xFF40; // LCD control
			public const ushort STAT = 0xFF41; // LCD status
			public const ushort SCY	 = 0xFF42; // scroll y
			public const ushort SCX	 = 0xFF43; // scroll x
			public const ushort LY   = 0xFF44; // LCD y co-ord
			public const ushort LYC	 = 0xFF45; // LCD y compare

			// DMA register
			public const ushort DMA	 = 0xFF46; 
			
			// palette data
			public const ushort BGP	 = 0xFF47; // background palette
			public const ushort OBP0 = 0xFF48; // object (sprite) palette 0
			public const ushort OBP1 = 0xFF49; // object (sprite) palette 1

			// LCD display registers
			public const ushort WY	 = 0xFF4A; // window y
			public const ushort WX   = 0xFF4B; // window x
		}

		private byte[] _vram;
		private byte[] _oam;

		private byte _lcdControl;					// LCDC value store
		private bool _displayEnabled;				// LCDC bit 7 - 0: Off / 1: On
		private ushort _windowTileBaseAddress;		// LCDC bit 6 - 0: 0x9800 - 0x9Bff / 1 : 0x9C00 = 0x9FFF
		private bool _windowEnabled;				// LCDC bit 5 - 0: Off / 1: On
		private ushort _backgroundCharBaseAddress;	// LCDC bit 4 - 0: 0x8800 - 0x97ff / 1 : 0x8000 = 0x8FFF
		private ushort _backgroundTileBaseAddress;	// LCDC bit 3 - 0: 0x9800 - 0x9Bff / 1 : 0x9C00 = 0x9FFF
		private int _spriteHeight;					// LCDC bit 2 - 0: 8x8 / 1: 8x16 (w*h)
		private bool _spritesEnabled;				// LCDC bit 1 - 0: Off / 1: On
		private bool _backgroundEnabled;			// LCDC bit 0 - 0: Off / 1: On

		private byte _lcdStatus;					// STAT value store

		private byte _scrollY, _scrollX;			// SCY, SCX

		private byte _currentScanline;				// LY
		private byte _scanlineCompare;				// LYC

		// palette data
		private byte _bgp;							// BGP value store
		private byte[] _backgroundPalette;			// background palette data

		private byte _obp0, _obp1;					// OBPn value store
		private byte[][] _spritePalette;			// object (sprite) palette data

		private byte _windowY, _windowX;			// WY, WX

		private uint _scanlineClocks; // counter of clock cycles elapsed in the current scanline

		public PPU(GameBoy gameBoy)
			: base(gameBoy)
		{
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
					return _lcdStatus;

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

						// reset LY when display is disabled
						if (!_displayEnabled)
						{
							_currentScanline = 0;
							CompareScanlineValue();
						}

						_windowTileBaseAddress = (ushort)((_lcdControl & 0x40) == 0x40 ? 0x9C00 : 0x9800);
						_windowEnabled = (_lcdControl & 0x20) == 0x20;
						_backgroundCharBaseAddress = (ushort)((_lcdControl & 0x10) == 0x10 ? 0x8000 : 0x8800);
						_backgroundTileBaseAddress = (ushort)((_lcdControl & 0x08) == 0x08 ? 0x9C00 : 0x9800);
						_spriteHeight = (_lcdControl & 0x04) == 0x04 ? 16 : 8;
						_spritesEnabled = (_lcdControl & 0x02) == 0x02;
						_backgroundEnabled = (_lcdControl & 0x01) == 0x01;

						break;

					case Registers.STAT:
						// capture lcd interrupt selection from value
						_lcdStatus = (byte)((_lcdStatus & 0x07) | (value & 0x78));

						// a write to the LY = LYC match flag clears the flag
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

		private byte[] ExtractPaletteData(byte value, int startingindex)
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

		public override void Update(ulong cycleCount)
		{
			// only run PPU update if display is enabled
			if (_displayEnabled)
			{
				_scanlineClocks += (uint)cycleCount;

				// 456 clocks a scanline
				if (_scanlineClocks >= 456)
				{
					_scanlineClocks -= 456;
					_currentScanline++;

					if (_currentScanline == 144)
					{
						_gb.RequestInterrupt(Interrupt.VBlank);
					}

					if (_currentScanline > 153)
					{
						_currentScanline = 0;
					}

					CompareScanlineValue();
				}
			}
		}
	}
}
