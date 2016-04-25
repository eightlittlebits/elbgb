using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_ui
{
	class DirectBitmap : IDisposable
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public Bitmap Bitmap { get; private set; }
		public IntPtr BitmapData { get; private set; }

		public DirectBitmap(int width, int height)
		{
			Width = width;
			Height = height;

			BitmapData = Marshal.AllocHGlobal(width * height * 4);
			Bitmap = new Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, BitmapData);
		}

		~DirectBitmap()
		{
			// Finalizer calls Dispose(false)
			Dispose(false);
		}

		public void Dispose()
		{
			// Dispose() calls Dispose(true)
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				if (Bitmap != null)
				{
					Bitmap.Dispose();
					Bitmap = null;
				}
			}

			// free unmanaged resources
			if (BitmapData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(BitmapData);
				BitmapData = IntPtr.Zero;
			}
		}
	}
}
