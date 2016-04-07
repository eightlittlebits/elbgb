using System;
using System.Collections.Generic;
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

			try
			{
				while (true)
				{
					gb.RunInstruction();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
