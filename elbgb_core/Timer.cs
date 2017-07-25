using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb_core.Memory;

namespace elbgb_core
{
    class Timer : ClockedComponent, IMemoryMappedComponent
    {
        public static class Registers
        {
            public const ushort DIV  = 0xFF04;
            public const ushort TIMA = 0xFF05;
            public const ushort TMA  = 0xFF06;
            public const ushort TAC  = 0xFF07;
        }

        const ushort f262144Hz = 0b0000001000;
        const ushort f65436Hz  = 0b0000100000;
        const ushort f16384Hz  = 0b0010000000;
        const ushort f4096Hz   = 0b1000000000;
        
        private InterruptController _interruptController;

        // internal frequency counter, DIV is the upper 8 bits
        private ushort _freqCounter;

        // timer
        private byte _tima, _tma, _tac;
        private bool _timerEnabled;

        private ushort _freqCounterMask;
        private bool _previousTimerUpdate;

        public Timer(SystemClock clock, Interconnect interconnect, InterruptController interruptController)
            : base(clock)
        {
            _interruptController = interruptController;

            interconnect.AddAddressHandler(0xFF04, 0xFF07, this);
        }

        public byte ReadByte(ushort address)
        {
            SynchroniseWithSystemClock();

            switch (address)
            {
                // the divider is the upper 8 bits of the 16-bit counter that counts the 
                // basic clock frequency (f). f = 4.194304 MHz
                case Registers.DIV: return (byte)(_freqCounter >> 8);

                // timer counter
                case Registers.TIMA:
                    return _tima;

                // timer modulo 
                case Registers.TMA:
                    return _tma;

                // timer controller
                case Registers.TAC:
                    return (byte)(_tac & 0x07);

                default:
                    throw new ArgumentOutOfRangeException("address");
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            SynchroniseWithSystemClock();

            switch (address)
            {
                // a write to DIV resets the register, regardless of value
                case Registers.DIV: _freqCounter = 0; break;

                // timer counter
                case Registers.TIMA:
                    _tima = value; break;

                // timer modulo 
                case Registers.TMA:
                    _tma = value; break;

                // timer controller
                case Registers.TAC:
                    _tac = (byte)(value & 0x07);

                    // update enabled state from bit 2
                    _timerEnabled = (value & 0x04) == 0x04;

                    // bits 0/1 specify timer frequency, set interval
                    switch (value & 0x03)
                    {
                        // 00 - f/2^10 (4.096 KHz)
                        case 0x00: _freqCounterMask = f4096Hz; break;

                        // 01 - f/2^4 (262.144 KHz)
                        case 0x01: _freqCounterMask = f262144Hz; break;

                        // 10 - f/2^6 (65.436 KHz)
                        case 0x02: _freqCounterMask = f65436Hz; break;

                        // 11 - f/2^8 (16.384 KHz)
                        case 0x03: _freqCounterMask = f16384Hz; break;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("address");
            }
        }

        public override void Update(uint cycleCount)
        {
            UpdateCounter(cycleCount % 8);
            UpdateTimer();

            for (int i = 0; i < cycleCount / 8; i++)
            {
                UpdateCounter(8);
                UpdateTimer();
            }
        }

        private void UpdateCounter(uint cycleCount)
        {
            _freqCounter += (ushort)cycleCount;
        }

        private void UpdateTimer()
        {
            bool updateTimer = _timerEnabled && ((_freqCounter & _freqCounterMask) == _freqCounterMask);
            
            // Check for the falling edge, ie previous is high and new low
            if (_previousTimerUpdate && !updateTimer)
            {
                _tima++;

                if (_tima == 0)
                {
                    _tima = _tma;

                    _interruptController.RequestInterrupt(Interrupt.TimerOverflow);
                }
            }

            _previousTimerUpdate = updateTimer;
        }
    }
}
