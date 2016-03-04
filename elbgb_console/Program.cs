using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy;

namespace elbgb_console
{
	class Program
	{
		static void Main(string[] args)
		{
			GameBoy gb = new GameBoy();

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

			Console.ReadLine();
		}
	}
}
