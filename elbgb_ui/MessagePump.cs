using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elbgb_ui
{
	static class MessagePump
	{
		private class IdleHandler
		{
			private Func<bool> _frameLoop;

			public IdleHandler(Func<bool> frameLoop)
			{
				_frameLoop = frameLoop;
			}

			internal void OnIdle(object sender, EventArgs e)
			{
                bool running = true;

				while (ApplicationStillIdle && running)
				{
					running = _frameLoop();
				}
			}
		}

		private static bool ApplicationStillIdle
		{
			get
			{
				NativeMethods.Message msg;
				return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
			}
		}

		public static void Run(Func<bool> frameLoop)
		{
			Application.Idle += new IdleHandler(frameLoop).OnIdle;
		}
	}
}
