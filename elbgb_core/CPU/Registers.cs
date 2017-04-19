using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.CPU
{
    class Registers
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

        public ref byte A { get => ref _af.hi; }
        public StatusFlags F { get { return (StatusFlags)_af.lo; } set { _af.lo = (byte)(value & FLAG_MASK_BYTE); } }
        public ushort AF { get { return _af.word; } set { _af.word = (ushort)(value & FLAG_MASK_WORD); } }

        public ref byte B { get => ref _bc.hi; }
        public ref byte C { get => ref _bc.lo; }
        public ref ushort BC { get => ref _bc.word; }

        public ref byte D { get => ref _de.hi; }
        public ref byte E { get => ref _de.lo; }
        public ref ushort DE { get => ref _de.word; }

        public ref byte H { get => ref _hl.hi; }
        public ref byte L { get => ref _hl.lo; }
        public ref ushort HL { get => ref _hl.word; }

        public ref ushort PC { get => ref _pc; }
        public ref ushort SP { get => ref _sp; }

        public ref bool IME { get => ref _ime; }
    }
}
