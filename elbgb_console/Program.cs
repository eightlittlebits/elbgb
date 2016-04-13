using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gbcore;

namespace elbgb_console
{
	class Program
	{
		static void Main(string[] args)
		{
			string romPath = args[0];
			byte[] rom = File.ReadAllBytes(romPath);
			
			GameBoy gb = new GameBoy();

			gb.LoadRom(rom);

			int frameCounter = 0;
			Stopwatch stopwatch = new Stopwatch();

			try
			{
				while (true)
				{
					stopwatch.Restart();

					gb.RunFrame();

					stopwatch.Stop();

					frameCounter++;

					double elapsedMicroseconds = stopwatch.ElapsedTicks / (double)(Stopwatch.Frequency / 1000);

					Console.WriteLine("Frame {0} ran in {1}ms {2}", frameCounter, elapsedMicroseconds, stopwatch.ElapsedMilliseconds);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
