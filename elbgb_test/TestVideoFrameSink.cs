using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using elbgb_core;

namespace elbgb_test
{
    class TestVideoFrameSink : IVideoFrameSink
    {
        const int Width = 160;
        const int Height = 144;

        uint[] _palette = { 0xFFEFEFEF, 0xFFB2B2B2, 0xFF757575, 0xFF383838 };

        byte[] _frameData = new byte[Width * Height];

        public void AppendFrame(byte[] frame)
        {
            Buffer.BlockCopy(frame, 0, _frameData, 0, frame.Length);
        }

        public string HashFrame()
        {
            byte[] hash;

            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(_frameData);
            }

            return string.Concat(hash.Select(x => x.ToString("x2")));
        }

        public void SaveFrame(Stream stream)
        {
            int imageWidth = Width;// * 2;
            int imageHeight = Height;// * 2;

            using (var bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb))
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    uint* scan0 = (uint*)bitmapData.Scan0.ToPointer();

                    for (int y = 0; y < imageHeight; y++)
                    {
                        for (int x = 0; x < imageWidth; x++)
                        {
                            //*scan0++ = _palette[_frameData[(y >> 1) * Width + (x >> 1)]];
                            *scan0++ = _palette[_frameData[y * Width + x]];
                        }
                    }
                }

                bitmap.UnlockBits(bitmapData);

                bitmap.Save(stream, ImageFormat.Png);
            }
        }
    }
}
