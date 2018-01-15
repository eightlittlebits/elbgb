namespace elbgb_core.Memory
{
    internal enum TriggerType
    {
        Read = 0x01,
        Write = 0x02,
        ReadOrWrite = Read | Write,
    }

    internal class TriggeredMemoryMapping : IMemoryMappedComponent
    {
        private readonly Interconnect _interconnect;
        private readonly uint _addressFrom;
        private readonly uint _addressTo;
        private readonly TriggerType _triggerType;
        private readonly IMemoryMappedComponent _component;

        public TriggeredMemoryMapping(Interconnect internconnect, uint addressFrom, uint addressTo, TriggerType triggerType, IMemoryMappedComponent component)
        {
            _interconnect = internconnect;
            _addressFrom = addressFrom;
            _addressTo = addressTo;
            _triggerType = triggerType;
            _component = component;
        }

        public byte ReadByte(ushort address)
        {
            if ((_triggerType & TriggerType.Read) == TriggerType.Read)
            {
                _interconnect.AddAddressHandler(_addressFrom, _addressTo, _component);
            }

            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            if ((_triggerType & TriggerType.Write) == TriggerType.Write)
            {
                _interconnect.AddAddressHandler(_addressFrom, _addressTo, _component);
            }
        }
    }
}