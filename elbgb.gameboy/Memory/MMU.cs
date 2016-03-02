using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.Display;

namespace elbgb.gameboy.Memory
{
	class MMU //: IMemoryMappedComponent
	{
		public static class Registers
		{
			public const ushort BOOTROMLOCK = 0xFF50;
		}

		private GameBoy _gb;

		private bool _bootRomLocked;
		private byte[] _bootRom;

		private byte[] _wram;
		private byte[] _hram;

		public MMU(GameBoy gameBoy, byte[] bootRom)
		{
			_gb = gameBoy;

			_bootRom = bootRom;
			_bootRomLocked = false;

			_wram = new byte[0x2000];
			_hram = new byte[0x7F];
		}

		public byte ReadByte(ushort address)
		{
			switch (address & 0xF000)
			{
				case 0x0000:
					if (!_bootRomLocked && address < 0x100)
					{
						return _bootRom[address];
					}

					throw new NotImplementedException();

				// working ram
				case 0xC000:
				case 0xD000:
					return _wram[address & 0x1FFF];

				case 0xF000:
					// timer IO registers
					if (address >= 0xFF04 && address <= 0xFF07)
					{
						return _gb.Timer.ReadByte(address);
					}
					// restricted area
					else if (address >= 0xFEA0 && address <= 0xFEFF)
					{
						return 0;
					}
					// hi ram
					else if (address >= 0xFF80 && address <= 0xFFFE)
					{
						return _hram[address & 0x7F];
					}
					else
						throw new NotImplementedException();

				default:
					throw new NotImplementedException();
			}
		}

		public void WriteByte(ushort address, byte value)
		{
			switch (address & 0xF000)
			{
				// working ram
				case 0xC000:
				case 0xD000:
					_wram[address & 0x1FFF] = value;
					return;

				// vram
				case 0x8000:
				case 0x9000:
					_gb.PPU.WriteByte(address, value);
					return;

				case 0xF000:
					// timer IO registers
					if (address >= 0xFF04 && address <= 0xFF07)
					{
						_gb.Timer.WriteByte(address, value);
						return;
					}
					// restricted area
					else if (address >= 0xFEA0 && address <= 0xFEFF)
					{
						return;
					}
					// hi ram
					else if (address >= 0xFF80 && address <= 0xFFFE)
					{
						_hram[address & 0x7F] = value;
						return;
					}
					else if (address == Registers.BOOTROMLOCK)
					{
						// TODO(david): do we set a value at this location? can we read from it?
						_bootRomLocked = true;
						return;
					}
					else
						throw new NotImplementedException();


				default:
					throw new NotImplementedException();
			}
		}
	}
}
