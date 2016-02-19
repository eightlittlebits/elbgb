using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.Memory;

namespace elbgb.gameboy.CPU
{
	class LR35902
	{
		private MMU _mmu;

		public LR35902(MMU mmu)
		{
			_mmu = mmu;
		}
	}
}
