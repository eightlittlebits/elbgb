using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace elbgb_ui
{
    class DirectBitmap : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Stride { get; }

        public Bitmap Bitmap { get; private set; }
        public IntPtr BitmapData { get; private set; }

        public DirectBitmap(int width, int height, PixelFormat pixelFormat)
        {
            Width = width;
            Height = height;

            int bytesPerPixel = (Image.GetPixelFormatSize(pixelFormat) + 7) / 8; // round to nearest byte
            Stride = 4 * ((width * bytesPerPixel + 3) / 4); // rounded up to a four-byte boundary

            BitmapData = Marshal.AllocHGlobal(Stride * height);
            Bitmap = new Bitmap(width, height, Stride, pixelFormat, BitmapData);
        }

        ~DirectBitmap()
        {
            Dispose(false);
        }

        public void Dispose()
        {
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
