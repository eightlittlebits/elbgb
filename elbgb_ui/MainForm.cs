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

namespace elbgb_ui
{
	public partial class MainForm : Form
	{
		private GameBoy _gameBoy;

		private const int ScreenWidth = 160;
		private const int ScreenHeight = 144;

		private byte[] _screenData;

		private Dictionary<string, int[]> _palettes;
		private Bitmap _displayBuffer;
		
		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_gameBoy = new GameBoy();

			_gameBoy.Interface.PresentScreenData = PresentScreenData;

			int width = ScreenWidth * 2;
			int height = ScreenHeight * 2 + mainFormMenuStrip.Height;

			this.ClientSize = new Size(width, height);

			InitialisePalettes();
			BuildPaletteMenu();

			_displayBuffer = CreateDisplayBuffer(160, 144);

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
			_gameBoy.RunFrame();
			
			PresentDisplayBuffer();
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

			foreach(var paletteName in _palettes.Keys)
			{
				var paletteMenuItem = new Components.ToolStripRadioButtonMenuItem(paletteName,
					null,
					(sender, e) =>
					{
						ApplyPaletteToImage(_displayBuffer, paletteName);
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
			_palettes = new Dictionary<string, int[]>
			{
				{"default", new int[] {0xFFFFFF, 0xB7B7B7, 0x686868, 0x000000} },

				// the following palettes from http://www.hardcoregaming101.net/gbdebate/gbcolours.htm
				{"dark yellow",	new int[] {0xFFF77B, 0xB5AE4A, 0x6B6931, 0x212010} },
				{"light yellow", new int[] {0xFFFF94, 0xD0D066, 0x949440, 0x666625} },
				{"green", new int[] {0xB7DC11, 0x88A808, 0x306030, 0x083808} },
				{"greyscale", new int[] {0xEFEFEF, 0xB2B2B2, 0x757575, 0x383838} },
				{"stark b/w", new int[] {0xFFFFFF, 0xB2B2B2, 0x757575, 0x000000} },
				{"gb pocket", new int[] {0xE3E6C9, 0xC3C4A5, 0x8E8B61, 0x6C6C4E} },
			};
		}

		private void ApplyPaletteToImage(Bitmap image, string paletteName)
		{
			#region parameter validation

			if (image == null)
				throw new ArgumentNullException("image");

			if ((image.PixelFormat & PixelFormat.Indexed) != PixelFormat.Indexed)
				throw new ArgumentException("Image must have an indexed pixel format", "image");

			if (paletteName == null)
				throw new ArgumentNullException("paletteName");

			if (!_palettes.ContainsKey(paletteName))
				throw new ArgumentException(string.Format("{0} is not a valid palette name.", paletteName), "paletteName");

			if (image.Palette.Entries.Length < _palettes[paletteName].Length)
				throw new ArgumentException(string.Format("Image bit depth too low to apply palette {0}", paletteName));

			#endregion

			var palette = image.Palette;

			for (int i = 0; i < _palettes[paletteName].Length; i++)
			{
				palette.Entries[i] = Color.FromArgb(_palettes[paletteName][i]);
			}

			image.Palette = palette;
		}

		private Bitmap CreateDisplayBuffer(int width, int height)
		{
			// TODO(david): Investigate performance, PixelFormat.Format32bppPArgb seems to be recommended
			var displayBuffer = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

			ApplyPaletteToImage(displayBuffer, "default");

			return displayBuffer;
		}

		public void PresentScreenData(byte[] screenData)
		{
			RenderScreenDataToBitmap(_displayBuffer, screenData);
		}

		private static void RenderScreenDataToBitmap(Bitmap bitmap, byte[] screenData)
		{
			// get a pinned GC handle to our screen data array so we can pass it as a 
			// user input buffer to LockBits
			GCHandle screenDataGCHandle = GCHandle.Alloc(screenData, GCHandleType.Pinned);
			try
			{
				BitmapData bitmapData = new BitmapData
				{
					Width = ScreenWidth,
					Height = ScreenHeight,
					Stride = ScreenWidth,
					PixelFormat = PixelFormat.Format8bppIndexed,
					Scan0 = screenDataGCHandle.AddrOfPinnedObject()
				};

				// pass our data to the bitmap
				bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
												ImageLockMode.WriteOnly | ImageLockMode.UserInputBuffer,
												bitmap.PixelFormat,
												bitmapData);

				// commit the changes and unlock the bitmap
				bitmap.UnlockBits(bitmapData);
			}
			finally
			{
				if (screenDataGCHandle.IsAllocated)
					screenDataGCHandle.Free();
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
