using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.CPU
{
    [StructLayout(LayoutKind.Explicit)]
    struct PairedRegister
    {
        [FieldOffset(0)] public ushort word;
        [FieldOffset(1)] public byte hi;
        [FieldOffset(0)] public byte lo;
    }
}
