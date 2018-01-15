using System;
using System.Runtime.CompilerServices;

namespace elbgb_core.CPU
{
    [Flags]
    enum StatusFlags : byte
    {
        Z = 0x80,
        N = 0x40,
        H = 0x20,
        C = 0x10,
    }

    static class StatusFlagExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FlagSet(this StatusFlags f, StatusFlags flag)
        {
            return (f & flag) == flag;
        }
    }
}
