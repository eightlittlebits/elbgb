using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	[Flags]
	enum Interrupt : byte
	{
		VBlank				= 1 << 0,
		LCDC				= 1 << 1,
		TimerOverflow		= 1 << 2,
		SerialIOComplete	= 1 << 3,
		Input				= 1 << 4,
	}
}
