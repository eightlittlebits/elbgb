namespace elbgb_core
{
    interface IMemoryMappedComponent
    {
        byte ReadByte(ushort address);
        void WriteByte(ushort address, byte value);
    }
}
