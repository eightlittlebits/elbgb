using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory
{
	class NullCartridge : Cartridge
	{
		public NullCartridge(CartridgeHeader header, byte[] romData)
			: base(header, romData)
		{

		}

		public override byte ReadByte(ushort address)
		{
			return 0;
		}

		public override void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
