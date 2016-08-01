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
        private bool _isRunning;

        private GameBoy _gameBoy;

        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;

        private Dictionary<string, uint[]> _palettes;
        private uint[] _activePalette;

        private byte[] _screenData;
        private DirectBitmap _displayBuffer;

        private long _lastFrameTimestamp;
        private readonly double TargetFrameTicks;

        private GBCoreInput _inputState;

        private bool _limitFrameRate;

        public MainForm()
        {
            InitializeComponent();

            InitialisePalettes();
            _activePalette = _palettes["greyscale"];

            TargetFrameTicks = Stopwatch.Frequency / (4194304 / 70224.0);

            _gameBoy = new GameBoy();

            _gameBoy.Interface.VideoRefresh = RefreshScreenData;
            _gameBoy.Interface.PollInput = ReturnInputState;

            _gameBoy.LoadRom(File.ReadAllBytes(@"roms\Legend of Zelda, The - Link's Awakening (USA, Europe).gb"));
            //_gameBoy.LoadRom(File.ReadAllBytes(@"roms\tetris.gb"));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int width = ScreenWidth * 2;
            int height = ScreenHeight * 2 + mainFormMenuStrip.Height;

            this.ClientSize = new Size(width, height);

            BuildPaletteMenu("greyscale");

            _screenData = new byte[ScreenWidth * ScreenHeight];
            _displayBuffer = new DirectBitmap(ScreenWidth, ScreenHeight);

            displayPanel.RealTimeUpdate = true;

            MessagePump.Run(Frame);

            _isRunning = true;
            _limitFrameRate = true;
        }

        protected override void OnActivated(EventArgs e)
        {
            _isRunning = true;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            _isRunning = false;
        }

        private GBCoreInput ReturnInputState()
        {
            return _inputState;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // returns true if the key was processed, if not pass to 
            // default handler
            if (!ProcessKeyboard(e.KeyCode, true))
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            // returns true if the key was processed, if not pass to 
            // default handler
            if (!ProcessKeyboard(e.KeyCode, false))
            {
                base.OnKeyUp(e);
            }
        }

        private bool ProcessKeyboard(Keys key, bool pressed)
        {
            switch (key)
            {
                case Keys.Down:
                    _inputState.Down = pressed;
                    break;

                case Keys.Left:
                    _inputState.Left = pressed;
                    break;

                case Keys.Right:
                    _inputState.Right = pressed;
                    break;

                case Keys.Up:
                    _inputState.Up = pressed;
                    break;

                case Keys.Z:
                    _inputState.B = pressed;
                    break;

                case Keys.X:
                    _inputState.A = pressed;
                    break;

                case Keys.Space:
                    _inputState.Start = pressed;
                    break;

                case Keys.ShiftKey:
                    _limitFrameRate = !pressed;
                    break;

                default:
                    return false;
            }

            return true;
        }

        private bool Frame()
        {
            if (!_isRunning)
            {
                // return false to break idle loop
                return false;
            }

            long updateTimeStart = Stopwatch.GetTimestamp();

            _gameBoy.StepFrame();

            long updateTimeEnd = Stopwatch.GetTimestamp();

            long renderTimeStart = updateTimeEnd;

            RenderScreenDataToDisplayBuffer();
            PresentDisplayBuffer();

            long renderTimeEnd = Stopwatch.GetTimestamp();

            long currentTimeStamp = Stopwatch.GetTimestamp();
            long elapsedTicks = currentTimeStamp - _lastFrameTimestamp;

            if (_limitFrameRate && elapsedTicks < TargetFrameTicks)
            {
                // get ms to sleep for, cast to int to truncate to nearest millisecond
                // take 1 ms off the sleep time as we don't always hit the sleep exactly, trade
                // burning extra cpu in the spin loop for accuracy
                int sleepMilliseconds = (int)((TargetFrameTicks - elapsedTicks) * 1000 / Stopwatch.Frequency) - 1;

                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }

                // spin for the remaining partial millisecond to hit target frame rate
                long sleepElapsed = Stopwatch.GetTimestamp();
                while ((sleepElapsed - _lastFrameTimestamp) < TargetFrameTicks)
                {
                    sleepElapsed = Stopwatch.GetTimestamp();
                }
            }

            long endFrameTimestamp = Stopwatch.GetTimestamp();

            long totalFrameTicks = endFrameTimestamp - _lastFrameTimestamp;

            _lastFrameTimestamp = endFrameTimestamp;

            double updateTime = (updateTimeEnd - updateTimeStart) * 1000 / (double)Stopwatch.Frequency;
            double renderTime = (renderTimeEnd - renderTimeStart) * 1000 / (double)Stopwatch.Frequency;

            double frameTime = totalFrameTicks * 1000 / (double)(Stopwatch.Frequency);

            this.Text = string.Format("elbgb - {0:00.000}ms {1:00.000}ms {2:00.0000}ms", updateTime, renderTime, frameTime);

            return true;
        }

        private void RefreshScreenData(byte[] screenData)
        {
            Buffer.BlockCopy(screenData, 0, _screenData, 0, ScreenWidth * ScreenHeight);
        }

        private unsafe void RenderScreenDataToDisplayBuffer()
        {
            uint* displayPixel = (uint*)_displayBuffer.BitmapData;

            fixed (uint* palette = _activePalette)
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
#if FALSE
                    if (!NativeMethods.StretchBlt(hdcDest, 0, 0, displayPanel.Width, displayPanel.Height,
													hdcSrc, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
													NativeMethods.TernaryRasterOperations.SRCCOPY))
#else
                    var blendFunction = new NativeMethods.BlendFunction(NativeMethods.AC_SRC_OVER, 0, 0x40, 0);

                    if (!NativeMethods.AlphaBlend(hdcDest, 0, 0, displayPanel.Width, displayPanel.Height,
                                                    hdcSrc, 0, 0, _displayBuffer.Width, _displayBuffer.Height,
                                                    blendFunction))
#endif
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
                {"default",         new uint[] {0xFFFFFFFF, 0xFFB7B7B7, 0xFF686868, 0xFF000000} },

				// the following palettes from http://www.hardcoregaming101.net/gbdebate/gbcolours.htm
				{"dark yellow",     new uint[] {0xFFFFF77B, 0xFFB5AE4A, 0xFF6B6931, 0xFF212010} },
                {"light yellow",    new uint[] {0xFFFFFF94, 0xFFD0D066, 0xFF949440, 0xFF666625} },
                {"green",           new uint[] {0xFFB7DC11, 0xFF88A808, 0xFF306030, 0xFF083808} },
                {"greyscale",       new uint[] {0xFFEFEFEF, 0xFFB2B2B2, 0xFF757575, 0xFF383838} },
                {"stark b/w",       new uint[] {0xFFFFFFFF, 0xFFB2B2B2, 0xFF757575, 0xFF000000} },
                {"gb pocket",       new uint[] {0xFFE3E6C9, 0xFFC3C4A5, 0xFF8E8B61, 0xFF6C6C4E} },
            };
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
