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
        internal Interconnect Interconnect;

        internal BootRom BootRom;
        internal SystemMemory Memory;

        internal SystemClock Clock;
        internal InterruptController InterruptController;
        internal LR35902 CPU;
        internal Timer Timer;
        internal LCDController LCD;
        internal SoundController PSG;
        internal InputController Input;
        internal SerialCommunicationController SerialIO;

        public Cartridge Cartridge;

        public GameBoy(IVideoFrameSink frameSink, IInputSource inputSource)
        {
            Clock = new SystemClock();
            Interconnect = new Interconnect();

            BootRom = new BootRom(Interconnect, BootRomType.Dmg);
            Memory = new SystemMemory(Interconnect);

            InterruptController = new InterruptController(Interconnect);

            CPU = new LR35902(Clock, Interconnect, InterruptController);
            Timer = new Timer(Clock, Interconnect, InterruptController);
            SerialIO = new SerialCommunicationController(Clock, Interconnect, InterruptController);
            LCD = new LCDController(Clock, Interconnect, InterruptController, frameSink);
            PSG = new SoundController(Clock, Interconnect);
            Input = new InputController(Interconnect, InterruptController, inputSource);

            Cartridge = Cartridge.LoadRom(Interconnect, null);
        }

        public void LoadRom(byte[] romData)
        {
            Cartridge = Cartridge.LoadRom(Interconnect, romData);
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
