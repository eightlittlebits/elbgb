namespace elbgb_core.Memory.Mappers
{
    class RomOnly : IMemoryBankController
    {
        //private Cartridge _cartridge;
        private byte[] _rom;

        public RomOnly(byte[] rom)
        {
            _rom = rom;
        }

        public byte ReadByte(ushort address)
        {
            if (address < 0x8000)
            {
                return _rom[address];
            }

            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address < 0x8000)
            {
                _rom[address] = value;
            }
        }
    }
}

