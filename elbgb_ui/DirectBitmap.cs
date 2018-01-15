using System;
using System.Drawing;
using System.Runtime.InteropServices;

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
