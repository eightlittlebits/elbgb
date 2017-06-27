using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
    public interface IInputSource
    {
        GBCoreInput PollInput();
    }
}
