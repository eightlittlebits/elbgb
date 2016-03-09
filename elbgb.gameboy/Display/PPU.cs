using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb.gameboy.Display
{
	class PPU : ClockedComponent
	{
		public static class Registers
		{
			public const ushort LCDC = 0xFF40;
			public const ushort STAT = 0xFF41;
			public const ushort SCY	 = 0xFF42;
			public const ushort SCX	 = 0xFF43;
			public const ushort LY   = 0xFF44;
			public const ushort LYC	 = 0xFF45;
			public const ushort DMA	 = 0xFF46;
			public const ushort BGP	 = 0xFF47;
			public const ushort OBP0 = 0xFF48;
			public const ushort OBP1 = 0xFF49;
			public const ushort WY	 = 0xFF4A;
			public const ushort WX   = 0xFF4B;
		}

		private byte[] _vram;
		private byte[] _oam;

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
