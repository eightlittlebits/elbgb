using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	class Timer : ClockedComponent
	{
		public static class Registers
		{
			public const ushort DIV  = 0xFF04;
			public const ushort TIMA = 0xFF05;
			public const ushort TMA  = 0xFF06;
			public const ushort TAC  = 0xFF07;
		}

		private ushort _div;

		public Timer(GameBoy gameBoy)
			: base(gameBoy)
		{

		}

		public byte ReadByte(ushort address)
		{
			SynchroniseWithSystemClock();

			switch (address)
			{
				case Registers.DIV: return (byte)(_div >> 8);

				default:
					throw new NotImplementedException();
			}
		}

		public void WriteByte(ushort address, byte value)
		{
			SynchroniseWithSystemClock();

			switch (address)
			{
				// a write always clears the upper 8 bits of DIV, regardless of value
				case Registers.DIV: _div &= 0x00FF; break;

				default:
					throw new NotImplementedException();
			}
		}

		public override void Update(ulong cycleCount)
		{
			UpdateDivider(cycleCount);
		}

		private void UpdateDivider(ulong cycleCount)
		{
			_div += (ushort)cycleCount;
		}
	}
}
