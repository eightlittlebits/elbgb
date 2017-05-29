using System.Runtime.InteropServices;

namespace elbgb_ui
{
    static partial class NativeMethods
    {
        internal static class WinMM
        {
            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
            public static extern uint TimeBeginPeriod(uint uMilliseconds);
        }
    }
}
