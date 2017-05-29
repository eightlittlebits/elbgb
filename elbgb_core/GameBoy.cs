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

		public SystemClock Clock;
		public LR35902 CPU;
		public MMU MMU;
		public Timer Timer;
		public LCDController LCD;
		public SoundController PSG;
		public InputController Input;
		public SerialCommunicationController SerialIO;

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

			CPU = new LR35902(this);
			MMU = new MMU(this);
			Timer = new Timer(this);
			LCD = new LCDController(this, frameSink);
			PSG = new SoundController(this);
			Input = new InputController(this);
			SerialIO = new SerialCommunicationController(this);

			Cartridge = Cartridge.LoadRom(null);
		}

		public void LoadRom(byte[] romData)
		{
			Cartridge = Cartridge.LoadRom(romData);
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

		internal void RequestInterrupt(Interrupt interrupt)
		{
			MMU.IF |= (byte)interrupt;
		}
	}
}
