using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static elbgb_ui.NativeMethods;

namespace elbgb_ui
{
	static class MessagePump
	{
        private static Action _idleLoop;
        private static bool _initialised;
        private static bool _running;

        private static bool ApplicationStillIdle => !User32.PeekMessage(out _, IntPtr.Zero, 0, 0, 0);

        public static void RunWhileIdle(Action idleLoop)
		{
            _idleLoop = idleLoop;
            _initialised = true;
            _running = true;

            Application.Idle += OnIdle;
		}

        public static void Pause() => _running = false;

        public static void Resume() => _running = _initialised && true;

        public static void Stop()
        {
            if (_initialised)
            {
                _running = false;
                _initialised = false;

                Application.Idle -= OnIdle;
            }
        }
        
        private static void OnIdle(object sender, EventArgs e)
        {
            while (_running && ApplicationStillIdle)
            {
                _idleLoop();
            }
        }
    }
}
