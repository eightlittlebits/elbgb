using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.CPU;
using elbgb.gameboy.Memory;
using elbgb.gameboy.Display;

namespace elbgb.gameboy
{
	public class GameBoy
	{
		private LR35902 _cpu;
		private MMU _mmu;
		private PPU _ppu;

		private ulong _clockCycles;
		private Timer _timer;

		internal ulong Timestamp { get { return _clockCycles; } }
		
		internal LR35902 CPU { get { return _cpu; } }
		internal Timer Timer { get { return _timer; } }
		internal MMU MMU { get { return _mmu; } }
		internal PPU PPU { get { return _ppu; } }

		public GameBoy()
		{
			_cpu = new LR35902(this);
			_timer = new Timer(this);
			_mmu = new MMU(this);
			_ppu = new PPU(this);
		}

		public void RunInstruction()
		{
			_cpu.ExecuteSingleInstruction();

			_timer.Update();
		}

		internal void AddMachineCycles(int cycleCount)
		{
			_clockCycles += (ulong)(4 * cycleCount);
		}
	}
}
