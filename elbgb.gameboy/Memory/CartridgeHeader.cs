using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy.Memory
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

	public struct CartridgeHeader
	{
		public byte[] EntryPoint;

		public byte[] NintendoLogo;
		
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
	}
}
