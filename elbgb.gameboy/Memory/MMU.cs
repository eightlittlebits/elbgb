using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy.Memory
{
	class MMU
	{
		private bool _bootRomLocked;
		private byte[] _bootRom;

		public MMU(byte[] bootRom)
		{
			this._bootRom = bootRom;
			this._bootRomLocked = false;
		}

		public byte ReadByte(ushort address)
		{
			if (!_bootRomLocked && address < 0x100)
			{
				return _bootRom[address];
			}

			throw new NotImplementedException();
		}
	}
}
