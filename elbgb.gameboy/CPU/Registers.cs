using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gbcore.CPU
{
	struct Registers
	{
		private const StatusFlags FLAG_MASK_BYTE = (StatusFlags)0xF0;
		private const ushort FLAG_MASK_WORD = 0xFFF0;

		private PairedRegister _af;
		private PairedRegister _bc;
		private PairedRegister _de;
		private PairedRegister _hl;

		private ushort _pc;
		private ushort _sp;

		private bool _ime;

		public byte A { get { return _af.hi; } set { _af.hi = value; } }
		public StatusFlags F { get { return (StatusFlags)_af.lo; } set { _af.lo = (byte)(value & FLAG_MASK_BYTE); } }
		public ushort AF { get { return _af.word; } set { _af.word = (ushort)(value & FLAG_MASK_WORD); } }

		public byte B { get { return _bc.hi; } set { _bc.hi = value; } }
		public byte C { get { return _bc.lo; } set { _bc.lo = value; } }
		public ushort BC { get { return _bc.word; } set { _bc.word = value; } }

		public byte D { get { return _de.hi; } set { _de.hi = value; } }
		public byte E { get { return _de.lo; } set { _de.lo = value; } }
		public ushort DE { get { return _de.word; } set { _de.word = value; } }

		public byte H { get { return _hl.hi; } set { _hl.hi = value; } }
		public byte L { get { return _hl.lo; } set { _hl.lo = value; } }
		public ushort HL { get { return _hl.word; } set { _hl.word = value; } }

		public ushort PC { get { return _pc; } set { _pc = value; } }
		public ushort SP { get { return _sp; } set { _sp = value; } }

		public bool IME { get { return _ime; } set { _ime = value; } }

	}
}
