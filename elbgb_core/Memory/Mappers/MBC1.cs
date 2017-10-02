using System;

namespace elbgb_core.Memory.Mappers
{
    class MBC1 : IMemoryBankController
    {
        private readonly byte[] _rom;
        private readonly byte[] _ram;

        private readonly int _romSizeMask;
        private readonly int _ramSizeMask;

        private int _bank1;
        private int _bank2;
        private int _mode;

        private bool _ramEnabled;

        private int _loRomOffset;
        private int _hiRomOffset;
        private int _ramOffset;

        public MBC1(byte[] rom, byte[] ram)
        {
            _rom = rom;
            _romSizeMask = rom.Length - 1;

            if (ram != null)
            {
                _ram = ram;
                _ramSizeMask = ram.Length - 1;
            }

            _bank1 = 1;
            _bank2 = 0;
            _mode = 0;

            UpdateOffsets();
        }

        public byte ReadByte(ushort address)
        {
            // ROM in the 0x0000-0x3FFF area
            if (address < 0x4000)
            {
                return _rom[(address + _loRomOffset) & _romSizeMask];
            }
            // ROM in the 0x4000-0x7FFF area
            else if (address < 0x8000)
            {
                return _rom[((address - 0x4000) + _hiRomOffset) & _romSizeMask];
            }
            // RAM in the 0xA000-0xBFFF area
            else if (_ramEnabled && address >= 0xA000 && address < 0xC000)
            {
                return _ram[((address - 0xA000) + _ramOffset) & _ramSizeMask];
            }

            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            // 0x0000-0x1FFF - RAM_EN - MBC1 RAM enable register
            if (address < 0x2000)
            {
                if (_ram != null && (value & 0x0F) == 0x0A)
                {
                    _ramEnabled = true;
                }
                else
                {
                    _ramEnabled = false;
                }
            }
            // 0x2000-0x3FFF - BANK1 - MBC1 bank register 1
            else if (address < 0x4000)
            {
                _bank1 = value & 0x1F;

                if (_bank1 == 0) _bank1 = 1;

                UpdateOffsets();
            }
            // 0x4000-0x5FFF - BANK2 - MBC1 bank register 2
            else if (address < 0x6000)
            {
                _bank2 = value & 0x03;

                UpdateOffsets();
            }
            // 0x6000-0x7FFF - MODE - MBC1 mode register
            else if (address < 0x8000)
            {
                _mode = value & 0x01;

                UpdateOffsets();
            }
            // RAM in the 0xA000-0xBFFF area
            else if (_ramEnabled && address >= 0xA000 && address < 0xC000)
            {
                _ram[((address - 0xA000) + _ramOffset) & _ramSizeMask] = value;
            }            
        }

        private void UpdateOffsets()
        {
            _hiRomOffset = ((_bank2 << 5) | _bank1) * Cartridge.RomBankSize;

            if (_mode == 0)
            {
                _loRomOffset = 0;
                _ramOffset = 0;
            }
            else
            {
                _loRomOffset = (_bank2 << 5) * Cartridge.RomBankSize;
                _ramOffset = _bank2 * Cartridge.RamBankSize;
            }

        }
    }
}

