using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using elbgb_core.Memory;

namespace elbgb_core
{
    class InputController : IMemoryMappedComponent
    {
        private InterruptController _interruptController;
        private IInputSource _inputSource;

        private GBCoreInput _inputState;

        private bool _p14;
        private bool _p15;

        private int _inputSelect;

        public InputController(Interconnect interconnect, InterruptController interruptController, IInputSource inputSource)
        {
            _interruptController = interruptController;
            _inputSource = inputSource;

            interconnect.AddAddressHandler(0xFF00, this);
        }

        public byte ReadByte(ushort address)
        {
            if (address == 0xFF00)
            {
                // request latest input from interface
                GBCoreInput newInputState = _inputSource.PollInput();

                // check to see if a button is pressed in this input state
                // that was not pressed the previous state, if so, raise the
                // input interrupt
                if (ButtonPressed(_inputState, newInputState))
                    _interruptController.RequestInterrupt(Interrupt.Input);

                _inputState = newInputState;

                int p14Value = 0x0F;
                int p15Value = 0x0F;

                if (_p14)
                {
                    p14Value = (_inputState.Down  ? 0 : (1 << 3)) |
                               (_inputState.Up    ? 0 : (1 << 2)) |
                               (_inputState.Left  ? 0 : (1 << 1)) |
                               (_inputState.Right ? 0 : (1 << 0));
                }

                if (_p15)
                {
                    p15Value = (_inputState.Start  ?  0 : (1 << 3)) |
                               (_inputState.Select ?  0 : (1 << 2)) |
                               (_inputState.B      ?  0 : (1 << 1)) |
                               (_inputState.A      ?  0 : (1 << 0));
                }

                return (byte)(0xC0 | _inputSelect | (p14Value & p15Value));
            }
            else
                throw new ArgumentException("Invalid memory address passed to InputController.ReadByte", "address");
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address == 0xFF00)
            {
                _inputSelect = (byte)(~value & 0x30);

                _p14 = (_inputSelect & 0x10) == 0x10;
                _p15 = (_inputSelect & 0x20) == 0x20;
            }
            else
                throw new ArgumentException("Invalid memory address passed to InputController.WriteByte", "address");
        }

        private bool ButtonPressed(GBCoreInput previousInputState, GBCoreInput newInputState)
        {
            // check each button, if any of them were not pressed previously and they are 
            // now we return true
            if ((!previousInputState.Up && newInputState.Up)
                || (!previousInputState.Down && newInputState.Down)
                || (!previousInputState.Left && newInputState.Left)
                || (!previousInputState.Right && newInputState.Right)
                || (!previousInputState.A && newInputState.A)
                || (!previousInputState.B && newInputState.B)
                || (!previousInputState.Start && newInputState.Start)
                || (!previousInputState.Select && newInputState.Select))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
