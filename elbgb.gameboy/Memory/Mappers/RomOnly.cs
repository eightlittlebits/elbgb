using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy.Memory.Mappers
{
	class RomOnly : Cartridge
	{
		public RomOnly(CartridgeHeader header, byte[] romData)
			: base(header, romData)
		{
			
		}

		public override byte ReadByte(ushort address)
		{
			return _romData[address];
		}

		public override void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
