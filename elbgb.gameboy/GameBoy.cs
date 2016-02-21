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
		private PPU _ppu;

		private MMU _mmu;
		private LR35902 _cpu;

		internal PPU PPU { get { return _ppu; } }
		
		public GameBoy(byte[] bootRom)
		{
			_ppu = new PPU(this);

			_mmu = new MMU(this, bootRom);
			_cpu = new LR35902(_mmu);
		}

		public void RunInstruction()
		{
			_cpu.ExecuteSingleInstruction();
		}
	}
}
