using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb_core.CPU;
using elbgb_core.Memory;
using elbgb_core.Sound;

namespace elbgb_core
{
    public class GameBoy
    {
        public GBCoreInterface Interface;

        internal Interconnect Interconnect;

        private SystemMemory _memory;

        internal SystemClock Clock;
        internal InterruptController InterruptController;
        internal LR35902 CPU;
        internal Timer Timer;
        internal LCDController LCD;
        internal SoundController PSG;
        internal InputController Input;
        internal SerialCommunicationController SerialIO;

        public Cartridge Cartridge;
        
        public GameBoy(IVideoFrameSink frameSink)
        {
            // default to an null interface, implementation by front ends is optional
            Interface = new GBCoreInterface
                {
                    PollInput = () => default(GBCoreInput),                    
                    SerialTransferComplete = serialData => { },
                };

            Clock = new SystemClock();
            Interconnect = new Interconnect();

            Interconnect.AddAddressHandler(0x0000, 0x00FF, new BootRom());
            _memory = new SystemMemory(this);
                        
            InterruptController = new InterruptController(this);

            CPU = new LR35902(this);
            Timer = new Timer(this);
            LCD = new LCDController(this, frameSink);
            PSG = new SoundController(this);
            Input = new InputController(this);
            SerialIO = new SerialCommunicationController(this);

            Cartridge = Cartridge.LoadRom(this, null);
        }

        public void LoadRom(byte[] romData)
        {
            Cartridge = Cartridge.LoadRom(this, romData);
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
