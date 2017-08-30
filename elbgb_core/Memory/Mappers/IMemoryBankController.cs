namespace elbgb_core.Memory.Mappers
{
    internal interface IMemoryBankController
    {
        byte ReadByte(ushort address);
        void WriteByte(ushort address, byte value);
    }

}
