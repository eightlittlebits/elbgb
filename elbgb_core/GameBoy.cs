﻿using elbgb_core.CPU;
using elbgb_core.Memory;
using elbgb_core.Sound;

namespace elbgb_core
{
    public class GameBoy
    {
        internal Interconnect Interconnect;

        internal SystemMemory Memory;

        internal SystemClock Clock;
        internal InterruptController InterruptController;
        internal SM83 CPU;
        internal Timer Timer;
        internal LCDController LCD;
        internal SoundController PSG;
        internal InputController Input;
        internal SerialCommunicationController SerialIO;

        internal Cartridge Cartridge;

        public GameBoy(IVideoFrameSink frameSink, IInputSource inputSource)
        {
            Clock = new SystemClock();
            Interconnect = new Interconnect();

            Interconnect.AddAddressHandler(0x0000, 0x00FF, new BootRom());
            Memory = new SystemMemory(Interconnect);

            InterruptController = new InterruptController(Interconnect);

            CPU = new SM83(Clock, Interconnect, InterruptController);
            Timer = new Timer(Clock, Interconnect, InterruptController);
            SerialIO = new SerialCommunicationController(Clock, Interconnect, InterruptController);
            LCD = new LCDController(Clock, Interconnect, InterruptController, frameSink);
            PSG = new SoundController(Clock, Interconnect);
            Input = new InputController(Interconnect, InterruptController, inputSource);
        }

        public void LoadRom(byte[] romData)
        {
            Cartridge = new Cartridge(Interconnect, romData);
        }

        public void RunFrame()
        {
            // 70224 cycles per frame (456 cycles per scanline * 154 scanlines)
            // calculate the next frame boundary (multiple of 70224) from the 
            // current timestamp and set that as the target
            ulong frameBoundary = Clock.Timestamp + (70224 - (Clock.Timestamp % 70224));

            while (Clock.Timestamp < frameBoundary)
            {
                Step();
            }
        }

        public void Step()
        {
            CPU.ExecuteInstruction();

            // synchronise hardware components with system clock after instruction
            Timer.SynchroniseWithSystemClock();
            LCD.SynchroniseWithSystemClock();
            PSG.SynchroniseWithSystemClock();
            SerialIO.SynchroniseWithSystemClock();
        }
    }
}
