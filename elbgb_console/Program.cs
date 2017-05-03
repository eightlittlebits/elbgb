using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb_core;

namespace elbgb_console
{
    class Program
    {
        class NullVideoSink : IVideoFrameSink
        {
            public void AppendFrame(byte[] frame)
            {
                return;
            }
        }

        static void Main(string[] args)
        {
            string romPath = args[0];

            //romPath = @"roms\cpu_instrs\cpu_instrs.gb";
            //romPath = @"roms\cpu_instrs\individual\01-special.gb";			// Passed
            //romPath = @"roms\cpu_instrs\individual\02-interrupts.gb";			// Passed
            //romPath = @"roms\cpu_instrs\individual\03-op sp,hl.gb";			// Passed
            //romPath = @"roms\cpu_instrs\individual\04-op r,imm.gb";			// Passed
            //romPath = @"roms\cpu_instrs\individual\05-op rp.gb";				// Passed
            //romPath = @"roms\cpu_instrs\individual\06-ld r,r.gb";				// Passed
            //romPath = @"roms\cpu_instrs\individual\07-jr,jp,call,ret,rst.gb";	// Passed
            //romPath = @"roms\cpu_instrs\individual\08-misc instrs.gb";		// Passed
            //romPath = @"roms\cpu_instrs\individual\09-op r,r.gb";				// Passed
            //romPath = @"roms\cpu_instrs\individual\10-bit ops.gb";			// Passed
            //romPath = @"roms\cpu_instrs\individual\11-op a,(hl).gb";			// Passed

            //romPath = @"roms\instr_timing\instr_timing.gb";					// Passed

            //romPath = @"roms\mem_timing\mem_timing.gb";						// Passed
            //romPath = @"roms\mem_timing\individual\01-read_timing.gb";		// Passed
            //romPath = @"roms\mem_timing\individual\02-write_timing.gb";		// Passed
            //romPath = @"roms\mem_timing\individual\03-modify_timing.gb";		// Passed

            byte[] rom = File.ReadAllBytes(romPath);

            GameBoy gb = new GameBoy(new NullVideoSink());

            //gb.Interface.SerialTransferComplete = OutputSerialValue;

            gb.LoadRom(rom);

            int frameCounter = 0;

            double maxFrameTime = 0;
            
            while (true)
            {
                long startFrame = Stopwatch.GetTimestamp();

					gb.StepFrame();

                long endFrame = Stopwatch.GetTimestamp();

                long totalFrameTicks = endFrame - startFrame;

                startFrame = endFrame;

                frameCounter++;

                double elapsedMilliseconds = totalFrameTicks * 1000 / (double)(Stopwatch.Frequency);

                if (elapsedMilliseconds > maxFrameTime)
                {
                    maxFrameTime = elapsedMilliseconds;
                }

                Console.WriteLine("Frame {0} ran in {1}ms ({2}ms)", frameCounter, elapsedMilliseconds, maxFrameTime);
            }
        }

        private static void OutputSerialValue(byte value)
        {
            Console.WriteLine("{0:X2}", value);
        }
    }
}
