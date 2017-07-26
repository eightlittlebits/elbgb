using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using elbgb_core;

using static elbgb_ui.NativeMethods;

namespace elbgb_ui
{
    internal class GdiRenderer : IVideoFrameSink, IDisposable
    {
        private Control _renderControl;
        private byte[] _screenData;
        private DirectBitmap _displayBuffer;
        private uint[] _palette;

        public uint[] Palette { get => _palette; set => _palette = value; }
        public byte[] ScreenData => _screenData;

        public GdiRenderer(int screenWidth, int screenHeight, Control renderControl, uint[] palette)
        {
            _renderControl = renderControl;
            _palette = palette;

            _screenData = new byte[screenWidth * screenHeight];
            _displayBuffer = new DirectBitmap(screenWidth, screenHeight);
        }

        public void AppendFrame(byte[] frame)
        {
            Debug.Assert(frame.Length == _screenData.Length);

            Buffer.BlockCopy(frame, 0, _screenData, 0, frame.Length);
        }

        public void RenderScreen()
        {
            RenderScreenDataToDisplayBuffer();
            PresentDisplayBuffer();
        }

        private unsafe void RenderScreenDataToDisplayBuffer()
        {
            uint* displayPixel = (uint*)_displayBuffer.BitmapData;

            fixed (uint* palette = _palette)
            fixed (byte* screenPtr = _screenData)
            {
                byte* screenPixel = screenPtr;

                for (int i = 0; i < _screenData.Length; i++)
                {
                    *displayPixel++ = palette[*screenPixel++];
                }
            }
        }

        private void PresentDisplayBuffer()
        {
            using (Graphics grDest = Graphics.FromHwnd(_renderControl.Handle))
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

                    hOldObject = Gdi32.SelectObject(hdcSrc, hBitmap);
                    if (hOldObject == IntPtr.Zero)
                        throw new Win32Exception();
#if true
                    if (!Gdi32.StretchBlt(hdcDest, 0, 0, _renderControl.Width, _renderControl.Height,
                                            hdcSrc, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
                                            Gdi32.TernaryRasterOperations.SRCCOPY))
#else
                    var perPixelAlphaBlend = new NativeMethods.BlendFunction
                    {
                        BlendOp = NativeMethods.AC_SRC_OVER,
                        BlendFlags = 0,
                        SourceConstantAlpha = 0xFF,
                        AlphaFormat = NativeMethods.AC_SRC_ALPHA
                    };

                    if (!NativeMethods.AlphaBlend(hdcDest, 0, 0, displayPanel.Width, displayPanel.Height,
                                                    hdcSrc, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
                                                    perPixelAlphaBlend))
#endif
                        throw new Win32Exception();
                }
                finally
                {
                    if (hOldObject != IntPtr.Zero) Gdi32.SelectObject(hdcSrc, hOldObject);
                    if (hBitmap != IntPtr.Zero) Gdi32.DeleteObject(hBitmap);
                    if (hdcDest != IntPtr.Zero) grDest.ReleaseHdc(hdcDest);
                    if (hdcSrc != IntPtr.Zero) grSrc.ReleaseHdc(hdcSrc);
                }
            }
        }

        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _displayBuffer.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}