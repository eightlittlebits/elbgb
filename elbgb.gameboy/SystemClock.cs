using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	class SystemClock
	{
		private ulong _clockCycles;

		public const uint ClockFrequency = 4194304;

		public ulong Timestamp { get { return _clockCycles; } }

		public void AddMachineCycles(uint machineCycleCount)
		{
			_clockCycles += machineCycleCount * 4;
		}
	}
}
