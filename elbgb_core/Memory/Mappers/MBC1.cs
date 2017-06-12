using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory.Mappers
{
    class MBC1 : Cartridge
    {
        private bool _ramEnabled;
        private int _romBank;
        private int _ramBank;
        private int _bankMode;

        private bool _hasRam;
        private byte[] _ram;

        public MBC1(GameBoy gameBoy, CartridgeHeader header, byte[] romData)
            : base(gameBoy, header, romData)
        {
            _ramEnabled = false;
            _romBank = 1;
            _ramBank = 0;
            _bankMode = 0;

            // generate appropriate sized RAM
            if (header.ExternalRamSize > 0)
            {
                _hasRam = true;

                switch (header.ExternalRamSize)
                {
                    case 2: _ram = new byte[0x2000]; break; // 64Kbit, 8KB
                    case 3: _ram = new byte[0x8000]; break; // 256Kbit, 32KB
                }
            }
        }

        public override void LoadExternalRam(Stream stream)
        {
            int index = 0;
            int count = (int)stream.Length;

            while (count > 0)
            {
                int n = stream.Read(_ram, index, count);

                index += n;
                count -= n;
            }
        }

        public override void SaveExternalRam(Stream stream)
        {
            if (_hasRam)
            {
                stream.Write(_ram, 0, _ram.Length); 
            }
        }

        public override byte ReadByte(ushort address)
        {
            // 0x0000 - 0x3FFF - ROM Bank 0
            if (address < 0x4000)
            {
                return _romData[address];
            }
            // 0x4000 - 0x7FFF - ROM Bank n
            else if (address < 0x8000)
            {
                return _romData[(address - 0x4000) + (_romBank * 0x4000)];
            }
            // 0xA000 - 0xBFFF - Expansion RAM
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
                    return _ram[(address - 0xA000) + (_ramBank * 0x2000)];
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
            //                Writing 0x0A to this enables RAM access
            // Register 1: 0x2000-0x3FFF - ROM Bank Select, values 0x01-0x1F
            // Register 2: 0x4000-0x5FFF - Upper ROM bank/RAM bank select, values 0-3
            //                Upper ROM bank code when using 8Mbits or more of ROM (and register 3 is 0)
            //                RAM bank code when using 256kbits of RAM (and register 3 is 1)
            // Register 3: 0x6000-0x7FFF - ROM/RAM Change, values 0-1
            //                When 0 Register 2 controls the upper ROM banks
            //                When 1 Register 2 controls the RAM bank select

            // expansion ram
            if (_ramEnabled && address >= 0xA000 && address < 0xC000)
            {
                // bank mode 0 - 16Mbit ROM / 64Kbit RAM, limited to first RAM bank
                if (_bankMode == 0)
                {
                    _ram[address - 0xA000] = value;
                }
                // bank mode 1 - 4MBit ROM / 256Kbit RAM, 4 RAM banks
                else
                {
                    _ram[(address - 0xA000) + (_romBank * 0x2000)] = value;
                }
            }
            // 0x0000 - 0x1FFF - register 0 - RAM enable
            else if (address < 0x2000)
            {
                if (_hasRam && value == 0x0A)
                    _ramEnabled = true;
                else
                    _ramEnabled = false;
            }
            // 0x2000 - 0x3FFF - register 1 - ROM bank select
            else if (address < 0x4000)
            {
                // register 1 sets the low 5 bits of the rom bank
                value &= 0x1F;

                if (value == 0) value = 1;

                _romBank &= ~0x1F;
                _romBank |= value;

            }
            // 0x4000 - 0x5FFF - register 2 - upper bank select
            else if (address < 0x6000)
            {
                value &= 0x03;

                // if we're 8mbit+ rom mode then the value sets the 
                // upper bits of the rom bank
                if (_bankMode == 0)
                {
                    _romBank &= 0x1F;
                    _romBank |= value << 5;
                }
                else
                {
                    _ramBank = value;
                }
            }
            // 0x6000 - 0x7FFF - register 3 - ROM/RAM change
            else if (address < 0x8000)
            {
                // TODO(david): do we need to clear the top bits of the ROM bank if switching to RAM banking?
                _bankMode = (value & 0x01);
            }
        }
    }
}
