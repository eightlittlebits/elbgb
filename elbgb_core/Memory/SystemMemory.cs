using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory
{
    class SystemMemory : IMemoryMappedComponent
    {
        private byte[] _wram = new byte[0x2000];
        private byte[] _hram = new byte[0x7F];

        public SystemMemory(GameBoy gameBoy)
        {
            gameBoy.Interconnect.AddAddressHandler(0xC000, 0xDFFF, this); // wram
            gameBoy.Interconnect.AddAddressHandler(0xE000, 0xFDFF, this); // wram mirror
            gameBoy.Interconnect.AddAddressHandler(0xFEA0, 0xFEFF, this); // restricted
            gameBoy.Interconnect.AddAddressHandler(0xFF80, 0xFFFE, this); // hram
        }

        public byte ReadByte(ushort address)
        {
            // 0xC000 - 0xDFFF - working ram
            // 0xE000 - 0xFDFF - mirrors working ram
            if (address >= 0xC000 && address <= 0xFDFF)
            {
                return _wram[address & 0x1FFF];
            }
            // 0xFEA0 - 0xFEFF - restricted area - return 0
            else if (address >= 0xFEA0 && address <= 0xFEFF)
            {
                return 0x00;
            }
            // 0xFF80 - 0xFFFE - hi ram
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                return _hram[address & 0x7F];
            }
            else
                throw new ArgumentOutOfRangeException(nameof(address));
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address >= 0xC000 && address <= 0xFDFF)
            {
                _wram[address & 0x1FFF] = value;
            }
            // 0xFEA0 - 0xFEFF - restricted area - return 0
            else if (address >= 0xFEA0 && address <= 0xFEFF)
            {
                return;
            }
            // 0xFF80 - 0xFFFE - hi ram
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                _hram[address & 0x7F] = value;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(address));
        }
    }
}
