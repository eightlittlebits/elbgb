using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gbcore.Memory.Mappers
{
	class RomOnly : Cartridge
	{
		public RomOnly(CartridgeHeader header, byte[] romData)
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
				return 0x00;
		}

		public override void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
