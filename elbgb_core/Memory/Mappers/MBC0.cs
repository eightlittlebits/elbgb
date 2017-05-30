using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory.Mappers
{
    class MBC0 : Cartridge
    {
        public MBC0(CartridgeHeader header, byte[] romData)
            : base(header, romData)
        {

        }

        public override void LoadExternalRam(Stream stream)
        {
            return;
        }

        public override void SaveExternalRam(Stream stream)
        {
            return;
        }

        public override byte ReadByte(ushort address)
        {
            if (address < 0x8000)
            {
                return _romData[address];
            }
            else
                return 0xFF;
        }

        public override void WriteByte(ushort address, byte value)
        {
            return;
        }
    }
}
