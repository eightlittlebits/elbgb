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
		private SystemClock _clock;
		private LR35902 _cpu;
		private MMU _mmu;
		private Timer _timer;
		private PPU _ppu;
		private PSG _psg;

		private ICartridge _romCartridge;

		internal SystemClock Clock { get { return _clock; } }
		internal LR35902 CPU { get { return _cpu; } }
		internal MMU MMU { get { return _mmu; } }
		internal Timer Timer { get { return _timer; } }
		internal PPU PPU { get { return _ppu; } }
		internal PSG PSG { get { return _psg; } }

		internal ICartridge Cartridge { get { return _romCartridge; } }

		public GameBoy()
		{
			_clock = new SystemClock();

			_cpu = new LR35902(this);
			_mmu = new MMU(this);
			_timer = new Timer(this);
			_ppu = new PPU(this);
			_psg = new PSG(this);

			_romCartridge = new NullCartridge();
		}

		public void RunInstruction()
		{
			_cpu.ProcessInterrupts();
			_cpu.ExecuteSingleInstruction();

			// synchronise hardware components with system clock after instruction
			_timer.SynchroniseWithSystemClock();
			_ppu.SynchroniseWithSystemClock();
			_psg.SynchroniseWithSystemClock();
		}

		internal void RequestInterrupt(Interrupt interrupt)
		{
			byte interruptRequest = _mmu.ReadByte(MMU.Registers.IF);

			interruptRequest |= (byte)interrupt;

			_mmu.WriteByte(MMU.Registers.IF, interruptRequest);
		}
	}
}
