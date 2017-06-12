using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory
{
    class UnmappedMemory : IMemoryMappedComponent
    {
        public void Initialise() { }

        public byte ReadByte(ushort address)
        {
            Debug.WriteLine($"Unmapped memory read at address 0x{address:X4}", "MEMORY");

            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            Debug.WriteLine($"Unmapped memory write, 0x{value:X2} at address 0x{address:X4}", "MEMORY");
        }
    }
}
