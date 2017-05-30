using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
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

        private GdiRenderer _renderer;

        private long _lastFrameTimestamp;

        private readonly double _stopwatchFrequency;
        private readonly double _targetFrameTicks;

        private GBCoreInput _inputState;

        private bool _limitFrameRate;

        private string _savePath;

        public MainForm()
        {
            InitializeComponent();

            InitialisePalettes();

            _stopwatchFrequency = Stopwatch.Frequency;
            _targetFrameTicks = _stopwatchFrequency / (4194304 / 70224.0);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int width = ScreenWidth * 2;
            int height = ScreenHeight * 2 + mainFormMenuStrip.Height;

            this.ClientSize = new Size(width, height);

            BuildPaletteMenu("greyscale");

            _renderer = new GdiRenderer(ScreenWidth, ScreenHeight, displayPanel, _palettes["greyscale"]);
            _gameBoy = new GameBoy(_renderer);

            _gameBoy.Interface.PollInput = ReturnInputState;

            string romPath = @"roms\Legend of Zelda, The - Link's Awakening (U) (V1.2) [!].gb";
            _gameBoy.LoadRom(File.ReadAllBytes(romPath));

            _savePath = Path.ChangeExtension(romPath, "sav");
            if (File.Exists(_savePath))
            {
                using (var stream = File.OpenRead(_savePath))
                {
                    _gameBoy.Cartridge.LoadExternalRam(stream);
                }
            }

            displayPanel.RealTimeUpdate = true;

            MessagePump.RunWhileIdle(Frame);

            _limitFrameRate = true;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            MessagePump.Resume();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            MessagePump.Pause();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            MessagePump.Stop();

            using (var stream = File.Open(_savePath, FileMode.Create, FileAccess.Write))
            {
                _gameBoy.Cartridge.SaveExternalRam(stream);
            }

            _renderer.Dispose();
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

        private void Frame()
        {
            long updateTimeStart = Stopwatch.GetTimestamp();

            _gameBoy.RunFrame();

            long updateTimeEnd = Stopwatch.GetTimestamp();

            long renderTimeStart = updateTimeEnd;

            _renderer.RenderScreen();

            long renderTimeEnd = Stopwatch.GetTimestamp();

            long currentTimeStamp = Stopwatch.GetTimestamp();
            long elapsedTicks = currentTimeStamp - _lastFrameTimestamp;

            if (_limitFrameRate && elapsedTicks < _targetFrameTicks)
            {
                // get ms to sleep for, cast to int to truncate to nearest millisecond
                // take 1 ms off the sleep time as we don't always hit the sleep exactly, trade
                // burning extra cpu in the spin loop for accuracy
                int sleepMilliseconds = (int)((_targetFrameTicks - elapsedTicks) * 1000 / _stopwatchFrequency) - 1;

                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }

                // spin for the remaining partial millisecond to hit target frame rate
                while ((Stopwatch.GetTimestamp() - _lastFrameTimestamp) < _targetFrameTicks) ;
            }

            long endFrameTimestamp = Stopwatch.GetTimestamp();

            long totalFrameTicks = endFrameTimestamp - _lastFrameTimestamp;

            _lastFrameTimestamp = endFrameTimestamp;

            double updateTime = (updateTimeEnd - updateTimeStart) * 1000 / _stopwatchFrequency;
            double renderTime = (renderTimeEnd - renderTimeStart) * 1000 / _stopwatchFrequency;

            double frameTime = totalFrameTicks * 1000 / _stopwatchFrequency;

            this.Text = $"elbgb - {updateTime:00.000}ms {renderTime:00.000}ms {frameTime:00.0000}ms";
        }

        private void BuildPaletteMenu(string defaultPalette)
        {
            foreach (var paletteName in _palettes.Keys)
            {
                var paletteMenuItem = new Components.ToolStripRadioButtonMenuItem(paletteName,
                    null,
                    (sender, e) =>
                    {
                        _renderer.Palette = _palettes[paletteName];
                    });

                if (paletteName == defaultPalette)
                {
                    paletteMenuItem.Checked = true;
                }

                paletteToolStripMenuItem.DropDownItems.Add(paletteMenuItem);
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

        private void checksumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Screen MD5: {CalculateMD5String(_renderer.ScreenData)}");
        }

        private static string CalculateMD5String(byte[] buffer)
        {
            byte[] hash;

            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(buffer);
            }

            return string.Concat(hash.Select(x => x.ToString("x2")));
        }
    }
}
