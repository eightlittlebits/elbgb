using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
	public class SystemClock
	{
		public const uint ClockFrequency = 4194304;

		public ulong Timestamp;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddMachineCycle()
		{
			Timestamp += 4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddMachineCycles(uint machineCycleCount)
		{
			Timestamp += machineCycleCount * 4;
		}
	}
}
