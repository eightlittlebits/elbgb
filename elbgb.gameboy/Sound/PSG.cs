using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy.Sound
{
	class PSG
	{
		public static class Registers
		{
			public const ushort NR10 = 0xFF10;
			public const ushort NR11 = 0xFF11;
			public const ushort NR12 = 0xFF12;
			public const ushort NR13 = 0xFF13;
			public const ushort NR14 = 0xFF14;

			public const ushort NR21 = 0xFF16;
			public const ushort NR22 = 0xFF17;
			public const ushort NR23 = 0xFF18;
			public const ushort NR24 = 0xFF19;

			public const ushort NR30 = 0xFF1A;
			public const ushort NR31 = 0xFF1B;
			public const ushort NR32 = 0xFF1C;
			public const ushort NR33 = 0xFF1D;
			public const ushort NR34 = 0xFF1E;

			public const ushort NR41 = 0xFF20;
			public const ushort NR42 = 0xFF21;
			public const ushort NR43 = 0xFF22;
			public const ushort NR44 = 0xFF23;

			public const ushort NR50 = 0xFF24;
			public const ushort NR51 = 0xFF25;
			public const ushort NR52 = 0xFF26;
		}

		private GameBoy _gb;

		public PSG(GameBoy gameBoy)
		{
			_gb = gameBoy;
		}

		public byte ReadByte(ushort address)
		{
			return 0;
		}

		public void WriteByte(ushort address, byte value)
		{
			return;
		}
	}
}
