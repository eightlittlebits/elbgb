﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb.gameboy.Display
{
	class PPU : ClockedComponent
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
			public const ushort BGP	 = 0xFF47;
			public const ushort OBP0 = 0xFF48;
			public const ushort OBP1 = 0xFF49;
			
			// LCD display registers
			public const ushort WY	 = 0xFF4A; // window y
			public const ushort WX   = 0xFF4B; // window x
		}

		private byte[] _vram;
		private byte[] _oam;

		private byte _lcdControl;
		private bool _displayEnabled;				// LCDC bit 7 - 0: Off / 1: On
		private ushort _windowTileBaseAddress;		// LCDC bit 6 - 0: 0x9800 - 0x9Bff / 1 : 0x9C00 = 0x9FFF
		private bool _windowEnabled;				// LCDC bit 5 - 0: Off / 1: On
		private ushort _backgroundCharBaseAddress;	// LCDC bit 4 - 0: 0x8800 - 0x97ff / 1 : 0x8000 = 0x8FFF
		private ushort _backgroundTileBaseAddress;	// LCDC bit 3 - 0: 0x9800 - 0x9Bff / 1 : 0x9C00 = 0x9FFF
		private int _spriteHeight;					// LCDC bit 2 - 0: 8x8 / 1: 8x16 (w*h)
		private bool _spritesEnabled;				// LCDC bit 1 - 0: Off / 1: On
		private bool _backgroundEnabled;			// LCDC bit 0 - 0: Off / 1: On

		private byte _scrollY, _scrollX;

		private uint _scanlineClocks; // counter of clock cycles elapsed in the current scanline
		
		private byte _currentScanline; 

		public PPU(GameBoy gameBoy)
			: base(gameBoy)
		{
			_vram = new byte[0x2000];
			_oam = new byte[0xA0];
		}

		public byte ReadByte(ushort address)
		{
			SynchroniseWithSystemClock();

			switch (address)
			{
				case Registers.LCDC:
					return _lcdControl;

				case Registers.SCY:
					return _scrollY;

				case Registers.SCX:
					return _scrollX;

				case Registers.LY:
					return _currentScanline;
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
						_windowTileBaseAddress = (ushort)((_lcdControl & 0x40) == 0x40 ? 0x9C00 : 0x9800);
						_windowEnabled = (_lcdControl & 0x20) == 0x20;
						_backgroundCharBaseAddress = (ushort)((_lcdControl & 0x10) == 0x10 ? 0x8000 : 0x8800);
						_backgroundTileBaseAddress = (ushort)((_lcdControl & 0x08) == 0x08 ? 0x9C00 : 0x9800);
						_spriteHeight = (_lcdControl & 0x04) == 0x04 ? 16 : 8; 
						_spritesEnabled = (_lcdControl & 0x02) == 0x02;
						_backgroundEnabled = (_lcdControl & 0x01) == 0x01;

						break;

					case Registers.SCY:
						_scrollY = value; break;

					case Registers.SCX:
						_scrollX = value; break;

					default:
						throw new ArgumentOutOfRangeException("address");
				}
			}
		}

		public override void Update(ulong cycleCount)
		{
			// update LY, advancing to next line
			_scanlineClocks += (uint)cycleCount;

			// 456 clocks a scanline
			if (_scanlineClocks >= 456)
			{
				_scanlineClocks -= 456;
				_currentScanline++;

				if (_currentScanline > 153)
				{
					_currentScanline = 0;
				}
			}
		}
	}
}
