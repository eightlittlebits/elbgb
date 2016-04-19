using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.CPU
{
	[Flags]
	enum StatusFlags : byte
	{
		Clear = 0x00,

		Z = 0x80,
		N = 0x40,
		H = 0x20,
		C = 0x10,
	}

	static class StatusFlagExtensions
	{
		public static bool FlagSet(this StatusFlags f, StatusFlags flag)
		{
			return (f & flag) == flag;
		}
	}
}
