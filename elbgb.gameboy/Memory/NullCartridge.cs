using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy.Memory
{
	class NullCartridge : ICartridge
	{
		public byte ReadByte(ushort address)
		{
			return 0;
		}

		public void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
