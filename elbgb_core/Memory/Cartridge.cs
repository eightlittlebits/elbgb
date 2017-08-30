using System;
using elbgb_core.Memory.Mappers;

namespace elbgb_core.Memory
{
    partial class Cartridge : IMemoryMappedComponent
    {
        public const int RomBankSize = 0x4000;
        public const int RamBankSize = 0x2000;

        private IMemoryBankController _mbc;

        private CartridgeHeader _header;
        private byte[] _rom;

        private bool _hasRam;
        private int _ramSize;
        private byte[] _ram;

        internal CartridgeHeader Header { get => _header; }
        public bool HasRam { get => _hasRam; }

        public Cartridge(Interconnect interconnect, byte[] romData)
        {
            interconnect.AddAddressHandler(0x0100, 0x7FFF, this); // rom
            interconnect.AddAddressHandler(0xA000, 0xBFFF, this); // ram

            interconnect.AddAddressHandler(0xFF50, new TriggeredMemoryMapping(interconnect, 0x0000, 0x00FF, TriggerType.Write, this));

            _header = new CartridgeHeader(romData);

            _rom = romData;

            // generate appropriate sized RAM
            if (_header.ExternalRamSize > 0)
            {
                _hasRam = true;

                switch (_header.ExternalRamSize)
                {
                    case 1: _ramSize = 0x00800; break; // 16Kbit, 2KB
                    case 2: _ramSize = 0x02000; break; // 64Kbit, 8KB
                    case 3: _ramSize = 0x08000; break; // 256Kbit, 32KB
                    case 4: _ramSize = 0x20000; break; // 1024Kbit, 128KB
                    case 5: _ramSize = 0x10000; break; // 512Kbit, 64KB

                    default:
                        throw new ArgumentOutOfRangeException("Unsupported RAM size specified");
                }

                _ram = new byte[_ramSize];
            }

            // create appropriate mapper
            switch (_header.CartridgeType)
            {
                case CartridgeType.RomOnly:
                    _mbc = new RomOnly(_rom);
                    break;

                case CartridgeType.Mbc1:
                case CartridgeType.Mbc1Ram:
                case CartridgeType.Mbc1RamBattery:
                    _mbc = new MBC1(_rom, _ram);
                    break;

                default:
                    throw new NotImplementedException($"Mapper {_header.CartridgeType} ({_header.CartridgeType:X}) not implemented.");
            }
        }
        
        public byte ReadByte(ushort address) => _mbc.ReadByte(address);

        public void WriteByte(ushort address, byte value) => _mbc.WriteByte(address, value);
    }
}
