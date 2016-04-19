using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gbcore.CPU;
using elbgb.gbcore.Memory;
using elbgb.gbcore.Sound;

namespace elbgb.gbcore
{
	public class GameBoy
	{
		// 70224 cycles per frame (456 cycles per scanline * 154 scanlines)
		private const int CyclesPerFrame = 70224;

		public GBCoreInterface Interface;

		public SystemClock Clock;
		public LR35902 CPU;
		public MMU MMU;
		public Timer Timer;
		public LCDController LCD;
		public PSG PSG;
		public SerialCommunicationController SerialIO;

		public Cartridge Cartridge;

		public GameBoy()
		{
			// default to an null interface, implementation by front ends is optional
			Interface = new GBCoreInterface
				{
					PresentScreenData = (byte[] screenData) => { },
					SerialTransferComplete  = (byte serialData) => { },
					//PollInput = () => { return default(GBCoreInput); }
				};

			Clock = new SystemClock();

			CPU = new LR35902(this);
			MMU = new MMU(this);
			Timer = new Timer(this);
			LCD = new LCDController(this);
			PSG = new PSG(this);
			SerialIO = new SerialCommunicationController(this);

			Cartridge = Cartridge.LoadRom(null);
		}

		public void LoadRom(byte[] romData)
		{
			Cartridge = Cartridge.LoadRom(romData);
		}

		public void RunFrame()
		{
			// add one frames cycles onto current system clock timestamp to get 
			// a target and run until reached
			ulong targetFrameTimestamp = Clock.Timestamp + CyclesPerFrame;

			while (Clock.Timestamp < targetFrameTimestamp)
			{
				RunInstruction();
			}
		}

		public void RunInstruction()
		{
			CPU.ProcessInterrupts();
			CPU.ExecuteSingleInstruction();

			// synchronise hardware components with system clock after instruction
			Timer.SynchroniseWithSystemClock();
			LCD.SynchroniseWithSystemClock();
			PSG.SynchroniseWithSystemClock();
			SerialIO.SynchroniseWithSystemClock();
		}

		internal void RequestInterrupt(Interrupt interrupt)
		{
			byte interruptRequest = MMU.ReadByte(MMU.Registers.IF);

			interruptRequest |= (byte)interrupt;

			MMU.WriteByte(MMU.Registers.IF, interruptRequest);
		}
	}
}
