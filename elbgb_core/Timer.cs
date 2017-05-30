using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
    public class Timer : ClockedComponent
    {
        public static class Registers
        {
            public const ushort DIV  = 0xFF04;
            public const ushort TIMA = 0xFF05;
            public const ushort TMA  = 0xFF06;
            public const ushort TAC  = 0xFF07;
        }

        // divider
        private ushort _divider;

        // timer
        private bool _timerEnabled;

        private uint _timerInterval;
        private uint _timerCounter;

        private byte _tima, _tma, _tac;
        
        public Timer(GameBoy gameBoy)
            : base(gameBoy)
        {

        }

        public byte ReadByte(ushort address)
        {
            SynchroniseWithSystemClock();

            switch (address)
            {
                // the divider is the upper 8 bits of the 16-bit counter that counts the 
                // basic clock frequency (f). f = 4.194304 MHz
                case Registers.DIV: return (byte)(_divider >> 8);

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
                // a write always clears the upper 8 bits of DIV, regardless of value
                case Registers.DIV: _divider &= 0x00FF; break;

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
                        case 0x00: _timerInterval = SystemClock.ClockFrequency / 4096; break;

                        // 01 - f/2^4 (262.144 KHz)
                        case 0x01: _timerInterval = SystemClock.ClockFrequency / 262144; break;

                        // 10 - f/2^6 (65.436 KHz)
                        case 0x02: _timerInterval = SystemClock.ClockFrequency / 65436; break;

                        // 11 - f/2^8 (16.384 KHz)
                        case 0x03: _timerInterval = SystemClock.ClockFrequency / 16384; break;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("address");
            }
        }

        public override void Update(uint cycleCount)
        {
            UpdateDivider(cycleCount);
            UpdateTimer(cycleCount);
        }

        private void UpdateDivider(uint cycleCount)
        {
            _divider += (ushort)cycleCount;
        }

        private void UpdateTimer(uint cycleCount)
        {
            if (_timerEnabled)
            {
                _timerCounter += cycleCount;

                while (_timerCounter >= _timerInterval)
                {
                    _tima++;
                    _timerCounter -= _timerInterval;

                    if (_tima == 0)
                    {
                        _tima = _tma;

                        _gb.RequestInterrupt(Interrupt.TimerOverflow);
                    }
                }
            }
        }
    }
}
