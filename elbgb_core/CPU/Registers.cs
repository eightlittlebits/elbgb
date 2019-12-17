using System.Runtime.InteropServices;

namespace elbgb_core.CPU
{
    [StructLayout(LayoutKind.Explicit)]
    internal
#if BIG_ENDIAN
    struct Registers
    {
        [FieldOffset(0)] public ushort AF;
        [FieldOffset(0)] public byte A;
        [FieldOffset(1)] public StatusFlags F;

        [FieldOffset(2)] public ushort BC;
        [FieldOffset(2)] public byte B;
        [FieldOffset(3)] public byte C;

        [FieldOffset(4)] public ushort DE;
        [FieldOffset(4)] public byte D;
        [FieldOffset(5)] public byte E;

        [FieldOffset(6)] public ushort HL;
        [FieldOffset(6)] public byte H;
        [FieldOffset(7)] public byte L;
    }
#else
    struct Registers
    {
        [FieldOffset(0)] public ushort AF;
        [FieldOffset(1)] public byte A;
        [FieldOffset(0)] public StatusFlags F;

        [FieldOffset(2)] public ushort BC;
        [FieldOffset(3)] public byte B;
        [FieldOffset(2)] public byte C;

        [FieldOffset(4)] public ushort DE;
        [FieldOffset(5)] public byte D;
        [FieldOffset(4)] public byte E;

        [FieldOffset(6)] public ushort HL;
        [FieldOffset(7)] public byte H;
        [FieldOffset(6)] public byte L;
    }
#endif
}
