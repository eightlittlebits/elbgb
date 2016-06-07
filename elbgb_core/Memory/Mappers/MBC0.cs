using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory.Mappers
{
	class MBC0 : Cartridge
	{
		public MBC0(CartridgeHeader header, byte[] romData)
			: base(header, romData)
		{
			
		}

		public override byte ReadByte(ushort address)
		{
			if (address < 0x8000)
			{
				return _romData[address];
			}
			else
				return 0xFF;
		}

		public override void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
