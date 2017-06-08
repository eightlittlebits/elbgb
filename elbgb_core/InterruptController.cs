using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
    class InterruptController : IMemoryMappedComponent
    {
        public static class Registers
        {
            public const ushort IF = 0xFF0F;
            public const ushort IE = 0xFFFF;
        }
        
        private byte _interruptEnable;
        private byte _interruptFlag;

        public byte IF
        {
            get => (byte)(_interruptFlag | 0xE0);
            set => _interruptFlag = value;
        }

        public byte IE
        {
            get => _interruptEnable;
            set => _interruptEnable = value;
        }

        public byte ReadByte(ushort address)
        {
            switch (address)
            {
                case Registers.IF:
                    return (byte)(_interruptFlag | 0xE0);

                case Registers.IE:
                    return _interruptEnable;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address));
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            switch (address)
            {
                case Registers.IF:
                    _interruptFlag = value;
                    break;

                case Registers.IE:
                    _interruptEnable = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address));
            }
        }

        internal void RequestInterrupt(Interrupt interrupt)
        {
            _interruptFlag |= (byte)interrupt;
        }
    }
}
