using System.Text;

namespace elbgb_core.Memory
{
    public enum CgbSupportCode : byte
    {
        Incompatible = 0x00,
        Compatible = 0x80,
        Exclusive = 0xC0
    }

    public enum CartridgeType : byte
    {
        RomOnly = 0x00,
        Mbc1 = 0x01,
        Mbc1Ram = 0x02,
        Mbc1RamBattery = 0x03,
        Mbc2 = 0x05,
        Mbc2Battery = 0x06,
        RomRam = 0x08,
        RomRamBattery = 0x09,
        MMM01 = 0x0B,
        MMM01Ram = 0x0C,
        MMM01RamBattery = 0x0D,
        Mbc3TimerBattery = 0x0F,
        Mbc3TimerRamBattery = 0x10,
        Mbc3 = 0x11,
        Mbc3Ram = 0x12,
        Mbc3RamBattery = 0x13,
        Mbc4 = 0x15,
        Mbc4Ram = 0x16,
        Mbc4RamBattery = 0x17,
        Mbc5 = 0x19,
        Mbc5Ram = 0x1A,
        Mbc5RamBattery = 0x1B,
        Mbc5Rumble = 0x1C,
        Mbc5RumbleRam = 0x1D,
        Mbc5RumbleRamBattery = 0x1E,
        PocketCamera = 0xFC,
        BandaiTama5 = 0xFD,
        HudsonHuC3 = 0xFE,
        HudsonHuC1 = 0xFF,
    }

    public class CartridgeHeader
    {
        public string GameTitle;
        public string GameCode;
        public CgbSupportCode CgbSupportCode;
        public string MakerCode;
        public byte SgbSupportCode;
        public CartridgeType CartridgeType;
        public byte RomSize;
        public byte ExternalRamSize;
        public byte DestinationCode;
        public byte OldLicenseeCode;
        public byte MaskRomVersion;

        public byte ComplementCheck;
        public ushort Checksum;

        public CartridgeHeader(byte[] romData)
        {
            // 0x134 - 0x13E - 11 ascii characters of the game title
            GameTitle = Encoding.ASCII.GetString(romData, 0x134, 11);

            // 0x13F - 0x142 - 4 ascii character game code
            GameCode = Encoding.ASCII.GetString(romData, 0x13F, 4);

            CgbSupportCode = (CgbSupportCode)romData[0x143];

            // 0x144 - 0x145 - 2 ascii character licensee code
            MakerCode = Encoding.ASCII.GetString(romData, 0x144, 2);

            SgbSupportCode = romData[0x146];
            CartridgeType = (CartridgeType)romData[0x147];
            RomSize = romData[0x148];
            ExternalRamSize = romData[0x149];
            DestinationCode = romData[0x14A];
            OldLicenseeCode = romData[0x14B];
            MaskRomVersion = romData[0x14C];

            ComplementCheck = romData[0x14D];

            Checksum = (ushort)((romData[0x14E] << 8) | romData[0x14F]);
        }
    }
}
