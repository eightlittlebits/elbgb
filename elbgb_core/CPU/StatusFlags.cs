using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.CPU
{
	[Flags]
	enum StatusFlags : byte
	{
		Z = 0x80,
		N = 0x40,
		H = 0x20,
		C = 0x10,
	}

	static class StatusFlagExtensions
	{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FlagSet(this StatusFlags f, StatusFlags flag)
		{
			return (f & flag) == flag;
		}
	}
}
