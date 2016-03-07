using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.CPU;
using elbgb.gameboy.Memory;
using elbgb.gameboy.Display;
using elbgb.gameboy.Sound;

namespace elbgb.gameboy
{
	public class GameBoy
	{
		// display refresh ~59.7 Hz, clock speed 4.194304 MHz, cycles per frame = 70224
		private const int CyclesPerFrame = 70224;

		private LR35902 _cpu;
		private MMU _mmu;
		private PPU _ppu;
		private PSG _psg;

		private ICartridge _romCartridge;

		private ulong _clockCycles;
		private Timer _timer;

		internal ulong Timestamp { get { return _clockCycles; } }

		internal LR35902 CPU { get { return _cpu; } }
		internal Timer Timer { get { return _timer; } }
		internal MMU MMU { get { return _mmu; } }
		internal PPU PPU { get { return _ppu; } }
		internal PSG PSG { get { return _psg; } }

		internal ICartridge Cartridge { get { return _romCartridge; } }

		public GameBoy()
		{
			_cpu = new LR35902(this);
			_mmu = new MMU(this);
			_timer = new Timer(this);
			_ppu = new PPU(this);
			_psg = new PSG(this);

			_romCartridge = new NullCartridge();
		}

		public void RunInstruction()
		{
			_cpu.ExecuteSingleInstruction();

			_timer.Update();
			_ppu.Update();
		}

		internal void AddMachineCycles(int cycleCount)
		{
			_clockCycles += (ulong)(4 * cycleCount);
		}
	}
}
