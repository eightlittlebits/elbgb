using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using elbgb_core;
using System.IO;
using System.Diagnostics;

namespace elbgb_ui
{
	public partial class MainForm : Form
	{
		private GameBoy _gameBoy;

		private const int ScreenWidth = 160;
		private const int ScreenHeight = 144;

		private Dictionary<string, uint[]> _palettes;
		private uint[] _activePalette;

		private Bitmap _displayBuffer;

		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_gameBoy = new GameBoy();

			_gameBoy.LoadRom(File.ReadAllBytes(@"roms\Super Mario Land (W) (V1.1) [!].gb"));

			//_gameBoy.LoadRom(File.ReadAllBytes(@"D:\GameboyTests\instr_timing\instr_timing.gb"));

			_gameBoy.Interface.VideoRefresh = RenderScreenDataDisplayBuffer;

			int width = ScreenWidth * 2;
			int height = ScreenHeight * 2 + mainFormMenuStrip.Height;

			this.ClientSize = new Size(width, height);

			InitialisePalettes();
			_activePalette = _palettes["default"];
			BuildPaletteMenu();

			_displayBuffer = CreateDisplayBuffer(ScreenWidth * 2, ScreenHeight * 2);

			displayPanel.RealTimeUpdate = true;

			Application.Idle += OnApplicationIdle;
		}

		private bool AppStillIdle
		{
			get
			{
				NativeMethods.Message msg;
				return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
			}
		}

		private void OnApplicationIdle(object sender, EventArgs e)
		{
			while (AppStillIdle)
			{
				Frame();
			}
		}

		private void Frame()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			_gameBoy.RunFrame();

			PresentDisplayBuffer();

			stopwatch.Stop();

			double elapsedMilliseconds = stopwatch.ElapsedTicks / (double)(Stopwatch.Frequency / 1000);

			this.Text = string.Format("elbgb - {0:.###}", elapsedMilliseconds);
		}

		private void PresentDisplayBuffer()
		{
			using (Graphics g = displayPanel.CreateGraphics())
			{
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

				g.DrawImage(_displayBuffer, 0, 0, displayPanel.Width, displayPanel.Height);
			}
		}

		private void BuildPaletteMenu()
		{
			var rootPaletteMenuItem = new ToolStripMenuItem("&Palette");

			mainFormMenuStrip.Items.Add(rootPaletteMenuItem);

			foreach (var paletteName in _palettes.Keys)
			{
				var paletteMenuItem = new Components.ToolStripRadioButtonMenuItem(paletteName,
					null,
					(sender, e) =>
					{
						_activePalette = _palettes[paletteName];
					});

				if (paletteName == "default")
				{
					paletteMenuItem.Checked = true;
				}

				rootPaletteMenuItem.DropDownItems.Add(paletteMenuItem);
			}
		}

		private void InitialisePalettes()
		{
			_palettes = new Dictionary<string, uint[]>
			{
				{"default", new uint[] {0xFFFFFF, 0xB7B7B7, 0x686868, 0x000000} },

				// the following palettes from http://www.hardcoregaming101.net/gbdebate/gbcolours.htm
				{"dark yellow",	new uint[] {0xFFF77B, 0xB5AE4A, 0x6B6931, 0x212010} },
				{"light yellow", new uint[] {0xFFFF94, 0xD0D066, 0x949440, 0x666625} },
				{"green", new uint[] {0xB7DC11, 0x88A808, 0x306030, 0x083808} },
				{"greyscale", new uint[] {0xEFEFEF, 0xB2B2B2, 0x757575, 0x383838} },
				{"stark b/w", new uint[] {0xFFFFFF, 0xB2B2B2, 0x757575, 0x000000} },
				{"gb pocket", new uint[] {0xE3E6C9, 0xC3C4A5, 0x8E8B61, 0x6C6C4E} },
			};
		}

		private Bitmap CreateDisplayBuffer(int width, int height)
		{
			return new Bitmap(width, height, PixelFormat.Format32bppPArgb);
		}

		private void RenderScreenDataDisplayBuffer(byte[] screenData)
		{
			BitmapData bitmapData = _displayBuffer.LockBits(new Rectangle(0, 0, _displayBuffer.Width, _displayBuffer.Height),
												ImageLockMode.WriteOnly,
												_displayBuffer.PixelFormat);

			try
			{
				unsafe
				{
					uint* ptr = (uint*)bitmapData.Scan0;

					for (int y = 0; y < 144 * 2; y++)
					{
						int screenY = y / 2;

						for (int x = 0; x < 160 * 2; x++)
						{
							int screenX = x / 2;

							*ptr++ = _activePalette[screenData[(screenY * ScreenWidth) + screenX]] | 0xFF000000;
						}

						// move to next line in bitmap
						ptr += bitmapData.Stride - bitmapData.Width * 4;
					}
				}
			}
			finally
			{
				_displayBuffer.UnlockBits(bitmapData);
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
