using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        private Bitmap _fadeBitmap;
        private Bitmap _screenBuffer;

        private uint[] _palette;

        public uint[] Palette { get => _palette; set => _palette = value; }

        public GdiRenderer(int screenWidth, int screenHeight, Control renderControl, uint[] palette)
        {
            _renderControl = renderControl;
            _palette = palette;

            _screenData = new byte[screenWidth * screenHeight];
            _displayBuffer = new DirectBitmap(screenWidth, screenHeight);

            _fadeBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppRgb);
            using (var fadeGraphics = Graphics.FromImage(_fadeBitmap))
            {
                fadeGraphics.Clear(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF));
            }

            _screenBuffer = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppRgb);
            using (var screenGraphics = Graphics.FromImage(_screenBuffer))
            {
                screenGraphics.Clear(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            }
        }

        public void AppendFrame(byte[] frame)
        {
            Debug.Assert(frame.Length == _screenData.Length);

            Buffer.BlockCopy(frame, 0, _screenData, 0, frame.Length);
        }

        public void RenderScreen()
        {
            RenderScreenDataToDisplayBuffer();
            FadeScreenBufferAndDrawDisplay();
            PresentDisplayBuffer(_screenBuffer);
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

        private void FadeScreenBufferAndDrawDisplay()
        {
            //using (Graphics grScreen = Graphics.FromImage(_screenBuffer))
            //using (Graphics grFade = Graphics.FromImage(_fadeBitmap))
            //{
            //    IntPtr hdcScreen = IntPtr.Zero;
            //    IntPtr hScreenBitmap = IntPtr.Zero;
            //    IntPtr hOldScreenObject = IntPtr.Zero;

            //    IntPtr hdcFade = IntPtr.Zero;
            //    IntPtr hFadeBitmap = IntPtr.Zero;
            //    IntPtr hOldFadeObject = IntPtr.Zero;

            //    try
            //    {
            //        hdcScreen = grScreen.GetHdc();
            //        hScreenBitmap = _screenBuffer.GetHbitmap();

            //        hOldScreenObject = Gdi32.SelectObject(hdcScreen, hScreenBitmap);
            //        if (hOldScreenObject == IntPtr.Zero)
            //            throw new Win32Exception();

            //        hdcFade = grFade.GetHdc();
            //        hFadeBitmap = _fadeBitmap.GetHbitmap();

            //        hOldFadeObject = Gdi32.SelectObject(hdcFade, hFadeBitmap);
            //        if (hOldFadeObject == IntPtr.Zero)
            //            throw new Win32Exception();

            //        var constantAlphaBlend = new Gdi32.BlendFunction
            //        {
            //            BlendOp = Gdi32.AC_SRC_OVER,
            //            SourceConstantAlpha = 0xFF,
            //        };

            //        if (!Gdi32.AlphaBlend(hdcScreen, 0, 0, _screenBuffer.Width, _screenBuffer.Height,
            //                                hdcFade, 0, 0, _fadeBitmap.Width, _fadeBitmap.Height,
            //                                constantAlphaBlend))
            //            throw new Win32Exception();
            //    }
            //    finally
            //    {
            //        if (hOldFadeObject != IntPtr.Zero) Gdi32.SelectObject(hdcFade, hOldFadeObject);
            //        if (hFadeBitmap != IntPtr.Zero) Gdi32.DeleteObject(hFadeBitmap);
            //        if (hdcFade != IntPtr.Zero) grFade.ReleaseHdc(hdcFade);

            //        if (hOldScreenObject != IntPtr.Zero) Gdi32.SelectObject(hdcScreen, hOldScreenObject);
            //        if (hScreenBitmap != IntPtr.Zero) Gdi32.DeleteObject(hScreenBitmap);
            //        if (hdcScreen != IntPtr.Zero) grScreen.ReleaseHdc(hdcScreen);
            //    }
            //}

            using (Graphics grScreen = Graphics.FromImage(_screenBuffer))
            using (Graphics grDisplay = Graphics.FromImage(_displayBuffer.Bitmap))
            {
                IntPtr hdcScreen = IntPtr.Zero;
                IntPtr hScreenBitmap = IntPtr.Zero;
                IntPtr hOldScreenObject = IntPtr.Zero;

                IntPtr hdcDisplay = IntPtr.Zero;
                IntPtr hDisplayBitmap = IntPtr.Zero;
                IntPtr hOldDisplayObject = IntPtr.Zero;

                try
                {
                    hdcScreen = grScreen.GetHdc();
                    //hScreenBitmap = _screenBuffer.GetHbitmap();

                    //hOldScreenObject = Gdi32.SelectObject(hdcScreen, hScreenBitmap);
                    //if (hOldScreenObject == IntPtr.Zero)
                    //    throw new Win32Exception();

                    hdcDisplay = grDisplay.GetHdc();
                    hDisplayBitmap = _displayBuffer.Bitmap.GetHbitmap();

                    hOldDisplayObject = Gdi32.SelectObject(hdcDisplay, hDisplayBitmap);
                    if (hOldDisplayObject == IntPtr.Zero)
                        throw new Win32Exception();

                    var perPixelAlphaBlend = new Gdi32.BlendFunction
                    {
                        BlendOp = Gdi32.AC_SRC_OVER,
                        BlendFlags = 0,
                        SourceConstantAlpha = 0xFF,
                        //AlphaFormat = Gdi32.AC_SRC_ALPHA
                    };

                    if (!Gdi32.AlphaBlend(hdcScreen, 0, 0, _screenBuffer.Width, _screenBuffer.Height,
                                            hdcDisplay, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
                                            perPixelAlphaBlend))
                        //if (!Gdi32.StretchBlt(hdcScreen, 0, 0, _screenBuffer.Width, _screenBuffer.Height,
                        //                        hdcDisplay, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
                        //                        Gdi32.TernaryRasterOperations.SRCCOPY))
                        throw new Win32Exception();
                }
                finally
                {
                    if (hOldDisplayObject != IntPtr.Zero) Gdi32.SelectObject(hdcDisplay, hOldDisplayObject);
                    if (hDisplayBitmap != IntPtr.Zero) Gdi32.DeleteObject(hDisplayBitmap);
                    if (hdcDisplay != IntPtr.Zero) grDisplay.ReleaseHdc(hdcDisplay);

                    if (hOldScreenObject != IntPtr.Zero) Gdi32.SelectObject(hdcScreen, hOldScreenObject);
                    if (hScreenBitmap != IntPtr.Zero) Gdi32.DeleteObject(hScreenBitmap);
                    if (hdcScreen != IntPtr.Zero) grScreen.ReleaseHdc(hdcScreen);
                }
            }
        }

        private void PresentDisplayBuffer(Bitmap display)
        {
            using (Graphics grDest = Graphics.FromHwnd(_renderControl.Handle))
            using (Graphics grSrc = Graphics.FromImage(display))
            {
                IntPtr hdcDest = IntPtr.Zero;
                IntPtr hdcSrc = IntPtr.Zero;
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr hOldObject = IntPtr.Zero;

                try
                {
                    hdcDest = grDest.GetHdc();
                    hdcSrc = grSrc.GetHdc();
                    hBitmap = display.GetHbitmap();

                    hOldObject = Gdi32.SelectObject(hdcSrc, hBitmap);
                    if (hOldObject == IntPtr.Zero)
                        throw new Win32Exception();

                    //var (width, height) = CalculateAspectRatioFit(_displayBuffer.Width, _displayBuffer.Height, _renderControl.ClientSize.Width, _renderControl.ClientSize.Height);

                    //int left = 0, top = 0;

                    //if (width < _renderControl.ClientSize.Width)
                    //    left = (_renderControl.ClientSize.Width - width) / 2;
                    //else
                    //    top = (_renderControl.ClientSize.Height - height) / 2;

#if true
                    if (!Gdi32.StretchBlt(hdcDest, 0, 0, _renderControl.ClientSize.Width, _renderControl.ClientSize.Height,
                    //if (!Gdi32.StretchBlt(hdcDest, left, top, width, height,
                                            hdcSrc, 0, 0, display.Width, display.Height,
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

        // https://stackoverflow.com/a/14731922
        // https://opensourcehacker.com/2011/12/01/calculate-aspect-ratio-conserving-resize-for-images-in-javascript/
        (int width, int height) CalculateAspectRatioFit(int sourceWidth, int sourceHeight, int destWidth, int destHeight)
        {
            var ratio = Math.Min((double)destWidth / sourceWidth, (double)destHeight / sourceHeight);

            return ((int)(sourceWidth * ratio), (int)(sourceHeight * ratio));
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