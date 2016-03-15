using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.Memory.Mappers;

namespace elbgb.gameboy.Memory
{
	abstract class Cartridge
	{
		public CartridgeHeader Header { get; set; }

		protected byte[] _romData;

		public static Cartridge LoadRom(byte[] romData)
		{
			if (romData == null)
			{
				return new NullCartridge(default(CartridgeHeader), null);
			}

			CartridgeHeader header = ReadCartridgeHeader(romData);

			switch (header.CartridgeType)
			{
				case CartridgeType.RomOnly:
					return new RomOnly(header, romData);

				case CartridgeType.Mbc1:
				case CartridgeType.Mbc1Ram:
				case CartridgeType.Mbc1RamBattery:
				case CartridgeType.Mbc2:
				case CartridgeType.Mbc2Battery:
				case CartridgeType.RomRam:
				case CartridgeType.RomRamBattery:
				case CartridgeType.MMM01:
				case CartridgeType.MMM01Ram:
				case CartridgeType.MMM01RamBattery:
				case CartridgeType.Mbc3TimerBattery:
				case CartridgeType.Mbc3TimerRamBattery:
				case CartridgeType.Mbc3:
				case CartridgeType.Mbc3Ram:
				case CartridgeType.Mbc3RamBattery:
				case CartridgeType.Mbc4:
				case CartridgeType.Mbc4Ram:
				case CartridgeType.Mbc4RamBattery:
				case CartridgeType.Mbc5:
				case CartridgeType.Mbc5Ram:
				case CartridgeType.Mbc5RamBattery:
				case CartridgeType.Mbc5Rumble:
				case CartridgeType.Mbc5RumbleRam:
				case CartridgeType.Mbc5RumbleRamBattery:
				case CartridgeType.PocketCamera:
				case CartridgeType.BandaiTama5:
				case CartridgeType.HudsonHuC3:
				case CartridgeType.HudsonHuC1:
				default:
					string message = string.Format("Mapper {0} ({0:X2}) not implemented.", header.CartridgeType);
					throw new NotImplementedException(message);
			}
		}

		protected Cartridge(CartridgeHeader header, byte[] romData)
		{
			Header = header;
			_romData = romData;
		}

		public abstract byte ReadByte(ushort address);
		public abstract void WriteByte(ushort address, byte value);

		private static CartridgeHeader ReadCartridgeHeader(byte[] romData)
		{
			CartridgeHeader header = default(CartridgeHeader);

			// 0x100 - 0x103 - entry point
			header.EntryPoint = new byte[4];
			header.EntryPoint[0] = romData[0x100];
			header.EntryPoint[1] = romData[0x101];
			header.EntryPoint[2] = romData[0x102];
			header.EntryPoint[3] = romData[0x103];

			// 0x104 - 0x133 - 48 byte nintendo logo
			header.NintendoLogo = new byte[48];
			for (int i = 0; i < 48; i++)
			{
				header.NintendoLogo[i] = romData[0x104 + i];
			}

			// 0x134 - 0x13E - 11 ascii characters of the game title
			header.GameTitle = Encoding.ASCII.GetString(romData, 0x134, 11);

			// 0x13F - 0x142 - 4 ascii character game code
			header.GameCode = Encoding.ASCII.GetString(romData, 0x13F, 4);

			header.CgbSupportCode = (CgbSupportCode)romData[0x143];

			// 0x144 - 0x145 - 2 ascii character licensee code
			header.MakerCode = Encoding.ASCII.GetString(romData, 0x144, 2);

			header.SgbSupportCode = romData[0x146];
			header.CartridgeType = (CartridgeType)romData[0x147];
			header.RomSize = romData[0x148];
			header.ExternalRamSize = romData[0x149];
			header.DestinationCode = romData[0x14A];
			header.OldLicenseeCode = romData[0x14B];
			header.MaskRomVersion = romData[0x14C];

			header.ComplementCheck = romData[0x14D];

			header.Checksum = (ushort)((romData[0x14E] << 8) | romData[0x14F]);

			return header;
		}
	}
}
