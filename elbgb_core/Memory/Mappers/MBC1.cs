using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory.Mappers
{
	class MBC1 : Cartridge
	{
		private bool _ramEnabled;
		private int _romBank;
		private int _upperBank;
		private int _bankMode;

		private bool _hasRam;
		private byte[] _ram;
		
		public MBC1(CartridgeHeader header, byte[] romData)
			: base(header, romData)
		{
			_ramEnabled = false;
			_romBank = 1;
			_upperBank = 0;
			_bankMode = 0;

			// generate appropriate sized RAM
			if (header.ExternalRamSize > 0)
			{
				_hasRam = true;

				switch (header.ExternalRamSize)
				{
					case 2: _ram = new byte[0x2000]; break;	// 64Kbit, 8KB
					case 3: _ram = new byte[0x8000]; break;	// 256Kbit, 32KB
				}
			}
		}

		public override byte ReadByte(ushort address)
		{
			if (address < 0x4000)
			{
				return _romData[address];
			}
			else if (address >= 0x4000 && address < 0x8000)
			{
				int selectedRomBank = _romBank;

				// if we're in 8Mbit+ mode then add in the upper ROM bank select
				if (_bankMode == 0)
					selectedRomBank |= _upperBank << 5;

				return _romData[(address - 0x4000) + (selectedRomBank * 0x4000)];
			}
			else if (_ramEnabled && address >= 0xA000 && address < 0xC000)
			{
				// bank mode 0 - 16Mbit ROM / 64Kbit RAM, limited to first RAM bank
				if (_bankMode == 0)
				{
					return _ram[address - 0xA000];
				}
				// bank mode 1 - 4MBit ROM / 256Kbit RAM, 4 RAM banks
				else
				{
					return _ram[(address - 0xA000) + (_upperBank * 0x2000)];
				}
			}
			else
				return 0xFF;
		}

		public override void WriteByte(ushort address, byte value)
		{
			// MBC1 has 4 hardware registers to control the mapping:
			//
			// Register 0: 0x0000-0x1FFF - RAM Enable, value 0x0A
			//				Writing 0x0A to this enables RAM access
			// Register 1: 0x2000-0x3FFF - ROM Bank Select, values 0x01-0x1F
			// Register 2: 0x4000-0x5FFF - Upper ROM bank/RAM bank select, values 0-3
			//				Upper ROM bank code when using 8Mbits or more of ROM (and register 3 is 0)
			//				RAM bank code when using 256kbits of RAM (and register 3 is 1)
			// Register 3: 0x6000-0x7FFF - ROM/RAM Change, values 0-1
			//				When 0 Register 2 controls the upper ROM banks
			//				When 1 Register 2 controls the RAM bank select

			// expansion ram
			if (_hasRam && _ramEnabled && address >= 0xA000 && address < 0xC000)
			{
				// bank mode 0 - 16Mbit ROM / 64Kbit RAM, limited to first RAM bank
				if (_bankMode == 0)
				{
					_ram[address - 0xA000] = value;
				}
				// bank mode 1 - 4MBit ROM / 256Kbit RAM, 4 RAM banks
				else
				{
					_ram[(address - 0xA000) + (_upperBank * 0x2000)] = value;
				}
			}
			// register 0 - RAM enable
			else if (address >= 0x0000 && address < 0x2000)
			{
				if (value == 0x0A)
					_ramEnabled = true;
				else
					_ramEnabled = false;
			}
			// register 1 - ROM bank select
			else if (address >= 0x2000 && address < 0x4000)
			{
				_romBank = value & 0x1F;

				if (_romBank == 0)
					_romBank++;
			}
			// register 2 - upper bank select
			else if (address >= 0x4000 && address < 0x6000)
			{
				_upperBank = value & 0x03;
			}
			// register 3 - ROM/RAM change
			else if (address >= 0x6000 && address < 0x8000)
			{
				_bankMode = value & 0x01;
			}
		}
	}
}
