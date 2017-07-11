using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory
{
    class Interconnect
    {
        UnmappedMemory _unmappedMemory;

        IMemoryMappedComponent[] _memory;

        public Interconnect()
        {
            _unmappedMemory = new UnmappedMemory();

            _memory = new IMemoryMappedComponent[0x10000];

            for (int i = 0; i < _memory.Length; i++)
            {
                _memory[i] = _unmappedMemory;
            }
        }
        
        public void AddAddressHandler(uint address, IMemoryMappedComponent handler) => _memory[address] = handler;

        public void AddAddressHandler(uint addressFrom, uint addressTo, IMemoryMappedComponent handler)
        {
            for (uint address = addressFrom; address <= addressTo; address++)
            {
                AddAddressHandler(address, handler);
            }
        }

        public void CopyAddressHandler(uint addressFrom, uint addressTo, uint handlerAddress)
        {
            IMemoryMappedComponent handler = _memory[handlerAddress];

            AddAddressHandler(addressFrom, addressTo, handler);
        }

        public byte ReadByte(ushort address) => _memory[address].ReadByte(address);
        
        public void WriteByte(ushort address, byte value) => _memory[address].WriteByte(address, value);
    }
}
