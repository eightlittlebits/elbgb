﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb.gameboy.Display
{
	class PPU
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

		private GameBoy _gb;

		private byte[] _vram;
		private byte[] _oam;

		public PPU(GameBoy gameBoy)
		{
			_gb = gameBoy;
			_vram = new byte[0x2000];
			_oam = new byte[0xA0];
		}

		public byte ReadByte(ushort address)
		{
			throw new NotImplementedException();
		}

		public void WriteByte(ushort address, byte value)
		{
			if (address >= 0x8000 && address <= 0x9fff)
			{
				_vram[address & 0x1FFF] = value;
			}
			else if (address >= 0xFE00 && address <= 0xFE9F)
			{
				_oam[address & 0xFF] = value;
			}
			else
				return;
		}
	}
}
