using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gbcore.CPU;
using elbgb.gbcore.Memory;
using elbgb.gbcore.Display;
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
		public PPU PPU;
		public PSG PSG;

		public Cartridge Cartridge;

		public GameBoy()
		{
			// default to an null interface, implementation by front ends is optional
			Interface = new GBCoreInterface
				{
					PresentScreenData = (byte[] screenData) => { },
					//PollInput = () => { return default(GBCoreInput); }
				};

			Clock = new SystemClock();

			CPU = new LR35902(this);
			MMU = new MMU(this);
			Timer = new Timer(this);
			PPU = new PPU(this);
			PSG = new PSG(this);

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
			PPU.SynchroniseWithSystemClock();
			PSG.SynchroniseWithSystemClock();
		}

		internal void RequestInterrupt(Interrupt interrupt)
		{
			byte interruptRequest = MMU.ReadByte(MMU.Registers.IF);

			interruptRequest |= (byte)interrupt;

			MMU.WriteByte(MMU.Registers.IF, interruptRequest);
		}
	}
}
