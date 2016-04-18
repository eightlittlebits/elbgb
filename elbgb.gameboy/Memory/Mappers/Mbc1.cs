using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gbcore.Memory.Mappers
{
	class Mbc1 : Cartridge
	{
		private int _currentRomBank;

		public Mbc1(CartridgeHeader header, byte[] romData)
			: base(header, romData)
		{
			_currentRomBank = 1;
		}

		public override byte ReadByte(ushort address)
		{
			if (address < 0x4000)
			{
				return _romData[address];
			}
			else if (address >= 0x4000 && address < 0x8000)
			{
				return _romData[(address - 0x4000) + (_currentRomBank * 0x4000)];
			}
			else
				return 0x00;
		}

		public override void WriteByte(ushort address, byte value)
		{
			// Writes to 0x2000 - 0x3FFF set the lower 5 bits of the current rom bank
			if (address >= 0x2000 && address < 0x4000)
			{
				// mask the current rom bank, preserving the upper 3 bits
				_currentRomBank &= 0xE0;

				// mask to preserve the lower 5 bits of the rom bank
				value &= 0x1F;

				// selection of bank 0 always select bank 1
				if (value == 0)
					value = 1;

				_currentRomBank |= value;
			}
			// Writes to 0x5000 - 0x5FFF set the ram bank number or specify the upper two bits of rom bank
			if (address >= 0x4000 && address < 0x6000)
			{
				// mask the current rom bank, preserving the upper 3 bits
				_currentRomBank &= 0xE0;

				// mask to preserve the lower 5 bits of the rom bank
				value &= 0x1F;

				// selection of bank 0 always select bank 1
				if (value == 0)
					value = 1;

				_currentRomBank |= value;
			}
		}
	}
}
