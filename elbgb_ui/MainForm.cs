using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elbgb_ui
{
	public partial class MainForm : Form
	{
		private Func<Bitmap> _imageGenerateMethod;

		private Bitmap _displayBuffer;

		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			int width = 160 * 2;
			int height = 144 * 2 + menuStrip1.Height;

			this.ClientSize = new Size(width, height);

			_imageGenerateMethod = Generate4bppIndexedImage;
			bppIndexedToolStripMenuItem.Checked = true;

			_displayBuffer = _imageGenerateMethod();
		}

		private Bitmap Generate4bppIndexedImage()
		{
			var rand = new Random();

			Bitmap image = new Bitmap(160, 144, PixelFormat.Format4bppIndexed);

			// set up our 2 bit palette
			var palette = image.Palette;

			palette.Entries[0] = Color.FromArgb(255, 255, 255, 255);
			palette.Entries[1] = Color.FromArgb(255, 183, 183, 183);
			palette.Entries[2] = Color.FromArgb(255, 104, 104, 104);
			palette.Entries[3] = Color.FromArgb(255, 0, 0, 0);

			image.Palette = palette;

			// fill bitmap with random palette entries 0-3
			var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
												ImageLockMode.WriteOnly,
												image.PixelFormat);

			unsafe
			{

				for (int y = 0; y < imageData.Height; y++)
				{
					byte* row = (byte*)imageData.Scan0 + (y * imageData.Stride);

					for (int x = 0; x < imageData.Width / 2; x++)
					{
						int pixelA = rand.Next(4);
						int pixelB = rand.Next(4);

						byte indexedPixel = (byte)((pixelA << 4) | pixelB);

						row[x] = indexedPixel;
					}
				}
			}

			image.UnlockBits(imageData);

			return image;
		}

		private Bitmap Generate32bppArgbImage()
		{
			var rand = new Random();

			Bitmap image = new Bitmap(160, 144, PixelFormat.Format32bppArgb);

			// fill bitmap with random colours
			var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
												ImageLockMode.WriteOnly,
												image.PixelFormat);

			var pixelFormatSize = Image.GetPixelFormatSize(image.PixelFormat) / 8;

			unsafe
			{
				for (int y = 0; y < imageData.Height; y++)
				{
					byte* row = (byte*)imageData.Scan0 + (y * imageData.Stride);

					for (int x = 0; x < imageData.Width; x++)
					{
						byte* pixel = row + (x * pixelFormatSize);

						pixel[0] = (byte)rand.Next(256); // B
						pixel[1] = (byte)rand.Next(256); // G
						pixel[2] = (byte)rand.Next(256); // R
						pixel[3] = (byte)rand.Next(256); // A
					}
				}
			}

			image.UnlockBits(imageData);

			return image;
		}

		private void displayPanel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			e.Graphics.DrawImage(_displayBuffer, 0, 0, displayPanel.Width, displayPanel.Height);
		}

		private void regenerateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_displayBuffer = _imageGenerateMethod();
			displayPanel.Invalidate();
		}

		private void bppIndexedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_imageGenerateMethod = Generate4bppIndexedImage;
			bppIndexedToolStripMenuItem.Checked = true;
			bppARGBToolStripMenuItem.Checked = false;

			_displayBuffer = _imageGenerateMethod();
			displayPanel.Invalidate();
		}

		private void bppARGBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_imageGenerateMethod = Generate32bppArgbImage;
			bppIndexedToolStripMenuItem.Checked = false;
			bppARGBToolStripMenuItem.Checked = true;

			_displayBuffer = _imageGenerateMethod();
			displayPanel.Invalidate();
		}
	}
}
