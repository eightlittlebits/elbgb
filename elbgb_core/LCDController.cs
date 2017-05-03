using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        [Flags]
        private enum SpriteAttributes : byte
        {
            SpritePalette = 0x10,
            HorizontalFlip = 0x20,
            VerticalFlip = 0x40,
            BackgroundPriority = 0x80
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Sprite
        {
            public byte Y;
            public byte X;
            public byte CharIdentifier;
            public SpriteAttributes Attributes;
        }

        public enum LcdMode : byte
        {
            HBlank = 0x00,
            VBlank = 0x01,
            OamRead = 0x02,
            VramRead = 0x03,
        }

        enum OamDmaStatus
        {
            Inactive,
            Requested,
            Active
        }

        struct OamDma
        {
            public OamDmaStatus Status;
            public ushort Source;
            public byte Address;

            public bool OamAvailable { get { return Address == 0x00; } }
        }

        private IVideoFrameSink _frameSink;

        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;

        private byte[] _screenData;

        private byte[] _vram;

        private byte[] _oam;
        private OamDma _oamDma;

        private byte _lcdControl;                   // LCDC value store
        private bool _displayEnabled;               // LCDC bit 7 - 0: Off / 1: On
        private ushort _windowTileBaseAddress;      // LCDC bit 6 - 0: 0x9800 - 0x9BFF / 1 : 0x9C00 = 0x9FFF
        private bool _windowEnabled;                // LCDC bit 5 - 0: Off / 1: On
        private ushort _backgroundCharBaseAddress;  // LCDC bit 4 - 0: 0x8800 - 0x97FF / 1 : 0x8000 = 0x8FFF
        private ushort _backgroundTileBaseAddress;  // LCDC bit 3 - 0: 0x9800 - 0x9BFF / 1 : 0x9C00 = 0x9FFF
        private int _spriteHeight;                  // LCDC bit 2 - 0: 8x8 / 1: 8x16 (w*h)
        private bool _spritesEnabled;               // LCDC bit 1 - 0: Off / 1: On
        private bool _backgroundEnabled;            // LCDC bit 0 - 0: Off / 1: On

        private bool _signedCharIdentifier;         // if the background char data is in the range 0x8800 - 0x97FF then tile identifiers are signed

        private byte _lcdStatus;                    // STAT value store
        private LcdMode _lcdMode;                   // STAT bit 0 - 1: LCD mode flag

        private byte _scrollY, _scrollX;            // SCY, SCX

        private byte _currentScanline;              // LY
        private byte _scanlineCompare;              // LYC

        // palette data
        private byte _bgp;                          // BGP value store
        private byte[] _backgroundPalette;          // background palette data

        private byte _obp0, _obp1;                  // OBPn value store
        private byte[][] _spritePalette;            // object (sprite) palette data

        private byte _windowY, _windowX;            // WY, WX

        private uint _frameClock;                   // counter of clock cycles elapsed in the current frame
        private uint _vblankClock;                  // counter of clock cycles elapsed in current scanline in vblank

        public LCDController(GameBoy gameBoy, IVideoFrameSink frameSink)
            : base(gameBoy)
        {
            _frameSink = frameSink;

            _screenData = new byte[ScreenWidth * ScreenHeight];

            _vram = new byte[0x2000];

            _oam = new byte[0xA0];
            _oamDma = default(OamDma);

            _backgroundPalette = new byte[4];

            _spritePalette = new byte[2][]
            {
                new byte[4],
                new byte[4]
            };
        }

        public byte ReadByte(ushort address)
        {
            SynchroniseWithSystemClock();

            // 0x8000 - 0x9FFF - vram
            if (address >= 0x8000 && address <= 0x9fff)
            {
                // cannot access vram in mode 3
                if (_lcdMode == LcdMode.VramRead)
                {
                    return 0xFF;
                }

                return _vram[address & 0x1FFF];
            }
            // 0xFE00 - 0xFE9F - OAM memory
            else if (address >= 0xFE00 && address <= 0xFE9F)
            {
                // cannot access oam in mode 2 or 3
                if (_lcdMode == LcdMode.OamRead || _lcdMode == LcdMode.VramRead || !_oamDma.OamAvailable)
                {
                    return 0xFF;
                }

                return _oam[address & 0xFF];
            }
            else
            {
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
            }

            throw new ArgumentOutOfRangeException("address");
        }

        public void WriteByte(ushort address, byte value)
        {
            SynchroniseWithSystemClock();

            // 0x8000 - 0x9FFF - vram
            if (address >= 0x8000 && address <= 0x9fff)
            {
                // cannot access vram in mode 3
                if (_lcdMode == LcdMode.VramRead)
                {
                    return;
                }

                _vram[address & 0x1FFF] = value;
            }
            // 0xFE00 - 0xFE9F - OAM memory
            else if (address >= 0xFE00 && address <= 0xFE9F)
            {
                // cannot access oam in mode 2 or 3
                if (_lcdMode == LcdMode.OamRead || _lcdMode == LcdMode.VramRead || !_oamDma.OamAvailable)
                {
                    return;
                }

                _oam[address & 0xFF] = value;
            }
            else
            {
                switch (address)
                {
                    case Registers.LCDC:
                        _lcdControl = value;

                        bool enableDisplay = (_lcdControl & 0x80) == 0x80;

                        // are we being asked to turn on the display and if so are we currently off?
                        if (enableDisplay && !_displayEnabled)
                        {
                            // lcd starts in mode 2 when enabled
                            _displayEnabled = true;
                            _lcdMode = LcdMode.OamRead;

                            CompareScanlineValue();
                        }
                        // reset LY when display is disabled
                        else if (!enableDisplay && _displayEnabled)
                        {
                            _displayEnabled = false;

                            _currentScanline = 0;

                            _frameClock = 0;
                            _lcdMode = 0;
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
                        _oamDma.Source = (ushort)(value << 8);
                        _oamDma.Status = OamDmaStatus.Requested;

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
            if (_oamDma.Status != OamDmaStatus.Inactive)
            {
                UpdateOamDma(cycleCount);
            }

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
                    if (_lcdMode != LcdMode.OamRead)
                    {
                        // We've just entered OAM read so we're at the beginning of the line,
                        // increment the current scanline unless we're coming from VBlank, then 
                        // don't inrecement because we've just set it to 0
                        if (_lcdMode != LcdMode.VBlank)
                        {
                            _currentScanline++;
                            CompareScanlineValue();
                        }

                        _lcdMode = LcdMode.OamRead;

                        // raise OAM stat interrupt if requested
                        if ((_lcdStatus & 0x20) == 0x20) _gb.RequestInterrupt(Interrupt.LCDCStatus);
                    }
                }
                // Mode 3 - OAM and VRAM read, 162-175 cycles of frame
                else if ((_frameClock % 456) < 252)
                {
                    if (_lcdMode != LcdMode.VramRead)
                    {
                        _lcdMode = LcdMode.VramRead;
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
                    _frameSink.AppendFrame(_screenData);

                    // clear the _screenData for the next frame
                    Array.Clear(_screenData, 0, _screenData.Length);
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

        private void UpdateOamDma(uint cycleCount)
        {
            uint dmaCycles = cycleCount / 4;

            for (int i = 0; i < dmaCycles; i++)
            {
                switch (_oamDma.Status)
                {
                    // this burns the first 4 cycles of setup when DMA is requested
                    case OamDmaStatus.Requested:
                        _oamDma.Address = 0x00;
                        _oamDma.Status = OamDmaStatus.Active;
                        break;

                    case OamDmaStatus.Active:
                        // have we completed the dma transfer
                        if (_oamDma.Address >= 0xA0)
                        {
                            _oamDma.Address = 0x00;
                            _oamDma.Status = OamDmaStatus.Inactive;
                            break;
                        }

                        // calculate address from source and current address
                        _oam[_oamDma.Address] = _gb.MMU.ReadByte((ushort)(_oamDma.Source | _oamDma.Address));
                        _oamDma.Address += 1;
                        break;

                    // have we changed the status to inactive inside of the loop?
                    case OamDmaStatus.Inactive:
                        return;
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

            if (_windowEnabled)
                RenderWindowScanline();

            if (_spritesEnabled)
                RenderSpriteScanline();
        }

		private unsafe void RenderBackgroundScanline()
		{
			int renderedScanline = (_currentScanline + _scrollY) & 0xFF;

			// from which tile row are we rendering?
			int tileRow = (renderedScanline >> 3) * 32;

			fixed(byte* screenPtr = _screenData)
			{
				// advance screen data write pointer to beginning of current scanline
				byte* scanlinePtr = screenPtr + (_currentScanline * 160);

				// draw 160 pixel scanline
				for (int x = 0; x < 160; x++)
				{
					// which tile column are we rendering
					int tileColumn = ((x + _scrollX) & 0xFF) >> 3;

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

					// which line in the tile?
					int line = (renderedScanline & 7);

					// generate address of the appropriate line in the tile
					// 16 bytes per character, 2 bytes per line
					// char ram base address + charIdentifier * 16 + line * 2
					int charDataAddress = _backgroundCharBaseAddress + (charIdentifier << 4) + (line << 1);

					// wrap char data address to vram memory size
					charDataAddress &= 0x1FFF;

					// decode character pixel data
					byte charData1 = _vram[charDataAddress];
					byte charData2 = _vram[charDataAddress + 1];

					int pixelOffset = 7 - ((x + _scrollX) & 7);

					var pixel = ((charData2 >> pixelOffset) & 0x01) << 1 | (charData1 >> pixelOffset) & 0x01;

					*scanlinePtr++ = _backgroundPalette[pixel];
				}
			}
        }

        private unsafe void RenderWindowScanline()
        {
            // are we in the window and is the window on screen?
            if (_currentScanline < _windowY || _windowX >= 167)
            {
                return;
            }

            int renderedScanline = _currentScanline - _windowY;

            // from which tile row are we rendering?
            int tileRow = (renderedScanline >> 3) * 32;

			fixed (byte* screenPtr = _screenData)
			{
				// advance screen data write pointer to beginning of current scanline
				byte* scanlinePtr = screenPtr + (_currentScanline * 160);
				
				// draw 160 pixel scanline
				for (int x = 0; x < 160; x++)
				{
					// if this is earlier in the scanline than the window x position
					// skip window rendering
					if (x < _windowX - 7)
						continue;

					// which tile column are we rendering
					int tileColumn = (x - (_windowX - 7)) >> 3;

					int tileAddress = _windowTileBaseAddress + tileRow + tileColumn;

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

					// which line in the tile?
					int line = (renderedScanline & 7);

					// generate address of the appropriate line in the tile
					// 16 bytes per character, 2 bytes per line
					// char ram base address + charIdentifier * 16 + line * 2
					int charDataAddress = _backgroundCharBaseAddress + (charIdentifier << 4) + (line << 1);

					// wrap char data address to vram memory size
					charDataAddress &= 0x1FFF;

					// decode character pixel data
					byte charData1 = _vram[charDataAddress];
					byte charData2 = _vram[charDataAddress + 1];

					int pixelOffset = 7 - ((x - (_windowX - 7)) & 7);

					var pixel = ((charData2 >> pixelOffset) & 0x01) << 1 | (charData1 >> pixelOffset) & 0x01;

					*scanlinePtr++ = _backgroundPalette[pixel];
				}
			}
        }

        private unsafe void RenderSpriteScanline()
        {
            Sprite[] renderList = new Sprite[10];

            int renderListCount = GenerateSpriteRenderList(renderList);

            // if we've no sprites on the current scanline there's nothing to render
            if (renderListCount == 0)
                return;

            // sort render list by x coords, lowest x has priority, if x is 
            // the same then leave in OAM order
            SortRenderList(renderList, renderListCount);

			fixed (byte* screenPtr = _screenData)
			{
				// advance screen data write pointer to beginning of current scanline
				byte* scanlinePtr = screenPtr + (_currentScanline * 160);
				
				// run through each of the sprites, in reverse order to maintain priority
				// higher priority sprites (earlier in list) will overdraw existing data
				for (int i = renderListCount - 1; i >= 0; i--)
				{
					Sprite sprite = renderList[i];

					// extract flip and priority flags from attributes
					bool flipY = (sprite.Attributes & SpriteAttributes.VerticalFlip) == SpriteAttributes.VerticalFlip;
					bool flipX = (sprite.Attributes & SpriteAttributes.HorizontalFlip) == SpriteAttributes.HorizontalFlip;
					bool backgroundPriority = (sprite.Attributes & SpriteAttributes.BackgroundPriority) == SpriteAttributes.BackgroundPriority;

					// get the correct palette based on the sprite attributes
					byte[] palette = (sprite.Attributes & SpriteAttributes.SpritePalette) == SpriteAttributes.SpritePalette ? _spritePalette[1] : _spritePalette[0];

					// which line in the final tile are we rendering?
					int line;

					if (flipY)
					{
						line = (_spriteHeight - ((_currentScanline - sprite.Y) & 0xFF) - 1) & 0xFF;
					}
					else
					{
						line = (_currentScanline - sprite.Y) & 0x0F;
					}

					// generate address of the appropriate line in the tile
					// 16 bytes per character, 2 bytes per line
					// char ram base address + charIdentifier * 16 + line * 2
					int charDataAddress = 0x8000 + (sprite.CharIdentifier << 4) + (line << 1);

					// wrap char data address to vram memory size
					charDataAddress &= 0x1FFF;

					// decode character pixel data
					byte charData1 = _vram[charDataAddress];
					byte charData2 = _vram[charDataAddress + 1];

					for (int p = 7; p >= 0; p--)
					{
						int pixelBit;

						if (flipX)
						{
							pixelBit = 7 - p;
						}
						else
						{
							pixelBit = p;
						}

						int pixel = ((charData2 >> pixelBit) & 0x01) << 1 | (charData1 >> pixelBit) & 0x01;

						// a pixel value of 0 is transparent for sprites, skip pixel
						if (pixel == 0)
						{
							continue;
						}

						byte targetX = (byte)(sprite.X + (7 - p));

						// if we're outside the bounds of the frame then skip this pixel
						// either spriteX + pixelX is out, meaning we're overlapping the 
						// right edge of the screen or the 
						if (targetX >= ScreenWidth)
						{
							continue;
						}

						// if the background has priority and is non zero then skip this pixel
						if (backgroundPriority && *(scanlinePtr + targetX) != 0)
						{
							continue;
						}

						*(scanlinePtr + targetX) = palette[pixel];
					}
				}
			}
        }

        private unsafe int GenerateSpriteRenderList(Sprite[] renderList)
        {
            int renderListCount = 0;

            fixed (byte* oamPtr = _oam)
            {
                // access oam as sprite records
                Sprite* sprite = (Sprite*)oamPtr;

                // loop through all 40 sprites and find the first 10 that are on 
                // the current scanline
                for (int i = 0; i < 40; i++)
                {
                    // x coord of 0x08 is displayed at left edge of screen
                    // y coord of 0x10 is displayed at the top edge of the screen
                    byte spriteX = (byte)(sprite->X - 0x08);
                    byte spriteY = (byte)(sprite->Y - 0x10);

                    if (((_currentScanline - spriteY) & 0xFF) < _spriteHeight)
                    {
                        renderList[renderListCount] = *sprite;

                        // set the x and y coords to be corrected screen space coords
                        renderList[renderListCount].Y = spriteY;
                        renderList[renderListCount].X = spriteX;

                        // if we've found 10 sprites then break from the loop
                        if (++renderListCount == 10)
                            break;
                    }

					sprite++;
                }
            }

            return renderListCount;
        }

        private static void SortRenderList(Sprite[] renderList, int renderListCount)
        {
            // the render list will only ever be at most 10 elements so use an insertion sort
            // https://en.wikipedia.org/wiki/Insertion_sort

            for (int i = 1; i < renderListCount; i++)
            {
                Sprite s = renderList[i];

                int j = i - 1;
                while (j >= 0 && renderList[j].X > s.X)
                {
                    renderList[j + 1] = renderList[j];
                    j--;
                }

                renderList[j + 1] = s;
            }
        }
    }
}
