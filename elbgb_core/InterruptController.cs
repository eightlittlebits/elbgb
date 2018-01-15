using System;
using elbgb_core.Memory;

namespace elbgb_core
{
    class InterruptController : IMemoryMappedComponent
    {
        public static class Registers
        {
            public const ushort IF = 0xFF0F;
            public const ushort IE = 0xFFFF;
        }

        public InterruptController(Interconnect interconnect)
        {
            interconnect.AddAddressHandler(Registers.IF, this);
            interconnect.AddAddressHandler(Registers.IE, this);
        }

        public byte IF;
        public byte IE;

        public byte ReadByte(ushort address)
        {
            switch (address)
            {
                case Registers.IF:
                    return IF;

                case Registers.IE:
                    return IE;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address));
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            switch (address)
            {
                case Registers.IF:
                    IF = (byte)(value | 0xE0);
                    break;

                case Registers.IE:
                    IE = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address));
            }
        }

        internal void RequestInterrupt(Interrupt interrupt)
        {
            IF |= (byte)interrupt;
        }
    }
}
