using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.Display;

namespace elbgb.gameboy.Memory
{
	class MMU : IMemoryMappedComponent
	{
		private GameBoy _gb;

		private bool _bootRomLocked;
		private byte[] _bootRom;

		public MMU(GameBoy gameBoy, byte[] bootRom)
		{
			_gb = gameBoy;

			_bootRom = bootRom;
			_bootRomLocked = false;
		}

		public byte ReadByte(ushort address)
		{
			if (!_bootRomLocked && address < 0x100)
			{
				return _bootRom[address];
			}
			// timer 0xFF04 - 0xFF07
			else if (address >= 0xFF04 && address <= 0xFF07)
			{
				return _gb.Timer.ReadByte(address);
			}

			throw new NotImplementedException();
		}

		public void WriteByte(ushort address, byte value)
		{
			if (address == 0xFF50) // Boot ROM lockout address
			{
				// TODO(david): do we set a value at this location? can we read from it?
				_bootRomLocked = true;
			}
			else if (address >= 0x8000 && address <= 0x9fff)
			{
				_gb.PPU.WriteByte(address, value);
			}
			// timer 0xFF04 - 0xFF07
			else if (address >= 0xFF04 && address <= 0xFF07)
			{
				_gb.Timer.WriteByte(address, value);
			}
			else
				throw new NotImplementedException();
		}
	}
}
