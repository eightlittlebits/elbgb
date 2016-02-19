using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.CPU;
using elbgb.gameboy.Memory;

namespace elbgb.gameboy
{
    public class GameBoy
    {
		private LR35902 _cpu;
		private MMU _mmu;

		internal MMU MMU { get { return _mmu; } }

		public GameBoy(byte[] bootRom)
		{
			_mmu = new MMU(bootRom);
			_cpu = new LR35902(_mmu);
		}
    }
}
