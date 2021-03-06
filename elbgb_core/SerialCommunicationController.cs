﻿using System;
using elbgb_core.Memory;

namespace elbgb_core
{
    class SerialCommunicationController : ClockedComponent, IMemoryMappedComponent
    {
        public static class Registers
        {
            public const ushort SB = 0xFF01;
            public const ushort SC = 0xFF02;
        }

        private InterruptController _interruptController;

        private readonly uint _internalShiftClockInterval;
        private bool _internalClock;

        private bool _transferRunning;
        private int _transferCounter;

        private uint _serialCounter;

        private byte _serialData;
        private byte _serialControl;
        
        public SerialCommunicationController(SystemClock clock, Interconnect interconnect, InterruptController interruptController)
            : base(clock)
        {
            _interruptController = interruptController;

            interconnect.AddAddressHandler(0xFF01, 0xFF02, this);

            // internal shift clock is 8192Hz 
            _internalShiftClockInterval = SystemClock.ClockFrequency / 8192;

            _internalClock = true;
        }

        public override void Update(uint cycleCount)
        {
            _serialCounter += cycleCount;

            while (_serialCounter >= _internalShiftClockInterval)
            {
                _serialCounter -= _internalShiftClockInterval;

                if (_transferRunning && _internalClock)
                {
                    _transferCounter++;

                    // run for 8 counts of the shift clock
                    if (_transferCounter == 8)
                    {
                        // transfer complete, reset control flag, output data and interrupt
                        _transferRunning = false;
                        _serialControl &= 0x7F;

                        // TODO(david): Work out how to signal a complete transfer externally

                        _interruptController.RequestInterrupt(Interrupt.SerialIOComplete);
                    }
                }
            }
        }

        public byte ReadByte(ushort address)
        {
            SynchroniseWithSystemClock();

            switch (address)
            {
                case Registers.SB:
                    if (!_transferRunning)
                        return _serialData;
                    else
                        // TODO(david): what does a serial read return when the transfer is in progress?
                        return 0xFF;

                case Registers.SC:
                    return (byte)(_serialControl | 0x7E);

                default:
                    throw new ArgumentOutOfRangeException("address");
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            SynchroniseWithSystemClock();

            switch (address)
            {
                case Registers.SB:
                    if (!_transferRunning)
                        _serialData = value;

                    break;

                case Registers.SC:
                    _serialControl = value;

                    // bit 1 sets internal or external shift clock source
                    // 0: external
                    // 1: internal
                    if ((value & 0x01) == 0x01)
                        _internalClock = true;
                    else
                        _internalClock = false;

                    // bit 7 starts a transfer
                    if ((value & 0x80) == 0x80)
                    {
                        _transferCounter = 0;
                        _transferRunning = true;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException("address");
            }
        }
    }
}
