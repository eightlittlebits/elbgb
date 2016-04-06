using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	public class SystemClock
	{
		public const uint ClockFrequency = 4194304;

		public ulong Timestamp;

		public void AddMachineCycles(uint machineCycleCount)
		{
			Timestamp += machineCycleCount * 4;
		}
	}
}
