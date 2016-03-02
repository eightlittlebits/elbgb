using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	class Timer
	{
		public static class Registers
		{
			public const ushort DIV  = 0xFF04;
			public const ushort TIMA = 0xFF05;
			public const ushort TMA  = 0xFF06;
			public const ushort TAC  = 0xFF07;
		}

		private GameBoy _gb;
		private ulong _lastUpdate;

		private ushort _div;

		public Timer(GameBoy gameBoy)
		{
			_gb = gameBoy;
		}

		public byte ReadByte(ushort address)
		{
			switch (address)
			{
				case Registers.DIV: return (byte)(_div >> 8);

				default:
					throw new NotImplementedException();
			}
		}

		public void WriteByte(ushort address, byte value)
		{
			switch (address)
			{
				// a write always clears the upper 8 bits of DIV, regardless of value
				case Registers.DIV: _div &= 0x00FF; break;

				default:
					throw new NotImplementedException();
			}
		}

		public void Update()
		{
			ulong cyclesToUpdate = _gb.Timestamp - _lastUpdate;

			_lastUpdate = _gb.Timestamp;

			UpdateDivider(cyclesToUpdate);
		}

		private void UpdateDivider(ulong cyclesToUpdate)
		{
			_div += (ushort)cyclesToUpdate;
		}
	}
}
