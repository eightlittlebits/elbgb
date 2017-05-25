using System.Runtime.InteropServices;

namespace elbgb_ui.NativeMethods
{
    static class WinMM
    { 
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);
    }
}
