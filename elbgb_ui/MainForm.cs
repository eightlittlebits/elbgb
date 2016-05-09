using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using elbgb_core;

namespace elbgb_ui
{
	public partial class MainForm : Form
	{
		private GameBoy _gameBoy;

		private const int ScreenWidth = 160;
		private const int ScreenHeight = 144;

		private Dictionary<string, uint[]> _palettes;
		private uint[] _activePalette;

		private byte[] _screenData;
		private DirectBitmap _displayBuffer;

		private long _lastFrameTimestamp;
		private double _targetFrameTicks;

		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_gameBoy = new GameBoy();

			_gameBoy.LoadRom(File.ReadAllBytes(@"roms\tetris.gb"));

			//_gameBoy.LoadRom(File.ReadAllBytes(@"D:\GameboyTests\instr_timing\instr_timing.gb"));

			_gameBoy.Interface.VideoRefresh = RefreshScreenData;

			int width = ScreenWidth * 2;
			int height = ScreenHeight * 2 + mainFormMenuStrip.Height;

			this.ClientSize = new Size(width, height);

			InitialisePalettes();
			_activePalette = _palettes["greyscale"];

			BuildPaletteMenu("greyscale");

			_screenData = new byte[ScreenWidth * ScreenHeight];
			_displayBuffer = CreateDisplayBuffer(ScreenWidth * 2, ScreenHeight * 2);

			displayPanel.RealTimeUpdate = true;

			Application.Idle += OnApplicationIdle;

			_targetFrameTicks = Stopwatch.Frequency / (4194304 / 70224.0);
			_lastFrameTimestamp = Stopwatch.GetTimestamp();
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
			_gameBoy.RunFrame();

			RenderScreenDataToDisplayBuffer();
			PresentDisplayBuffer();

			long currentTimeStamp = Stopwatch.GetTimestamp();
			long elapsedTicks = currentTimeStamp - _lastFrameTimestamp;

			if (elapsedTicks < _targetFrameTicks)
			{
				// get ms to sleep for, cast to int to truncate to nearest millisecond
				int sleepMilliseconds = (int)((_targetFrameTicks - elapsedTicks) * 1000 / Stopwatch.Frequency);

				Thread.Sleep(sleepMilliseconds);
			}

			while ((Stopwatch.GetTimestamp() - _lastFrameTimestamp) < _targetFrameTicks)
			{
				// spin for the remaining partial millisecond to hit target frame rate
			}

			long endFrameTimestamp = Stopwatch.GetTimestamp();

			long elapsedAfterSleep = endFrameTimestamp - _lastFrameTimestamp;
			
			_lastFrameTimestamp = endFrameTimestamp;

			double elapsedMilliseconds = elapsedAfterSleep * 1000 / (double)(Stopwatch.Frequency);
			double framesPerSecond = Stopwatch.Frequency / (double)elapsedAfterSleep;

			this.Text = string.Format("elbgb - {0:.###}ms {1:.####}fps", elapsedMilliseconds, framesPerSecond);			
		}

		private void RefreshScreenData(byte[] screenData)
		{
			Buffer.BlockCopy(screenData, 0, _screenData, 0, ScreenWidth * ScreenHeight);
		}

		private unsafe void RenderScreenDataToDisplayBuffer()
		{
			uint* ptr = (uint*)_displayBuffer.BitmapData;

			fixed (uint* palette = _activePalette)
			fixed (byte* screenPtr = _screenData)
			{
				byte* rowPtr;
				int screenY, screenX;

				for (int y = 0; y < 144 * 2; y++)
				{
					screenY = (y >> 1) * ScreenWidth;
					rowPtr = screenPtr + screenY;

					for (int x = 0; x < 160 * 2; x++)
					{
						screenX = x >> 1;

						*ptr++ = *(palette + *(rowPtr + screenX));
					}
				}
			}
		}

		private void PresentDisplayBuffer()
		{
			using (Graphics grDest = Graphics.FromHwnd(displayPanel.Handle))
			using (Graphics grSrc = Graphics.FromImage(_displayBuffer.Bitmap))
			{
				IntPtr hdcDest = IntPtr.Zero;
				IntPtr hdcSrc = IntPtr.Zero;
				IntPtr hBitmap = IntPtr.Zero;
				IntPtr hOldObject = IntPtr.Zero;

				try
				{
					hdcDest = grDest.GetHdc();
					hdcSrc = grSrc.GetHdc();
					hBitmap = _displayBuffer.Bitmap.GetHbitmap();

					hOldObject = NativeMethods.SelectObject(hdcSrc, hBitmap);
					if (hOldObject == IntPtr.Zero)
						throw new Win32Exception();

					if (!NativeMethods.BitBlt(hdcDest, 0, 0, displayPanel.Width, displayPanel.Height,
						hdcSrc, 0, 0, NativeMethods.RasterOperation.SRCCOPY))
						throw new Win32Exception();
				}
				finally
				{
					if (hOldObject != IntPtr.Zero) NativeMethods.SelectObject(hdcSrc, hOldObject);
					if (hBitmap != IntPtr.Zero) NativeMethods.DeleteObject(hBitmap);
					if (hdcDest != IntPtr.Zero) grDest.ReleaseHdc(hdcDest);
					if (hdcSrc != IntPtr.Zero) grSrc.ReleaseHdc(hdcSrc);
				}
			}
		}

		private void BuildPaletteMenu(string defaultPalette)
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

				if (paletteName == defaultPalette)
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
				{"default",			new uint[] {0xFFFFFFFF, 0xFFB7B7B7, 0xFF686868, 0xFF000000} },

				// the following palettes from http://www.hardcoregaming101.net/gbdebate/gbcolours.htm
				{"dark yellow",		new uint[] {0xFFFFF77B, 0xFFB5AE4A, 0xFF6B6931, 0xFF212010} },
				{"light yellow",	new uint[] {0xFFFFFF94, 0xFFD0D066, 0xFF949440, 0xFF666625} },
				{"green",			new uint[] {0xFFB7DC11, 0xFF88A808, 0xFF306030, 0xFF083808} },
				{"greyscale",		new uint[] {0xFFEFEFEF, 0xFFB2B2B2, 0xFF757575, 0xFF383838} },
				{"stark b/w",		new uint[] {0xFFFFFFFF, 0xFFB2B2B2, 0xFF757575, 0xFF000000} },
				{"gb pocket",		new uint[] {0xFFE3E6C9, 0xFFC3C4A5, 0xFF8E8B61, 0xFF6C6C4E} },
			};
		}

		private DirectBitmap CreateDisplayBuffer(int width, int height)
		{
			return new DirectBitmap(width, height);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
