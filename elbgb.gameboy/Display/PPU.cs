using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace elbgb.gameboy.Display
{
	class PPU : IMemoryMappedComponent
	{
		private GameBoy _gb;

		private byte[] _vram;

		public PPU(GameBoy gameBoy)
		{
			_gb = gameBoy;
			_vram = new byte[0x2000];
		}

		public byte ReadByte(ushort address)
		{
			throw new NotImplementedException();
		}

		public void WriteByte(ushort address, byte value)
		{
			if (address >= 0x8000 && address <= 0x9fff)
			{
				_vram[address - 0x8000] = value;
			}
			else
				throw new NotImplementedException();
		}
	}
}
