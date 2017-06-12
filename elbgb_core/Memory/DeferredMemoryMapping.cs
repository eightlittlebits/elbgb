using System;

namespace elbgb_core.Memory
{
    internal class DeferredMemoryMapping : IMemoryMappedComponent
    {
        private Interconnect _interconnect;
        private uint _addressFrom;
        private uint _addressTo;
        private IMemoryMappedComponent _component;

        public DeferredMemoryMapping(Interconnect internconnect, uint addressFrom, uint addressTo, IMemoryMappedComponent component)
        {
            _interconnect = internconnect;
            _addressFrom = addressFrom;
            _addressTo = addressTo;
            _component = component;
        }

        public byte ReadByte(ushort address)
        {
            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            // when the address this deferred memory mapping is written to
            // we apply the memory mapping passed to us on creation
            _interconnect.AddAddressHandler(_addressFrom, _addressTo, _component);
        }
    }
}