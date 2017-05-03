using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
    [Flags]
    enum Interrupt : byte
    {
        VBlank           = 0b0000_0001,
        LCDCStatus       = 0b0000_0010,
        TimerOverflow    = 0b0000_0100,
        SerialIOComplete = 0b0000_1000,
        Input            = 0b0001_0000,
    }
}
