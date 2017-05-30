using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
    public abstract class ClockedComponent
    {
        protected GameBoy _gb;
        protected ulong _lastUpdate;

        public ClockedComponent(GameBoy gameBoy)
        {
            _gb = gameBoy;
        }

        public void SynchroniseWithSystemClock()
        {
            // if we're up to date with the current timestamp there
            // is nothing for us to do
            if (_lastUpdate == _gb.Clock.Timestamp)
            {
                return;
            }

            ulong timestamp = _gb.Clock.Timestamp;
            uint cyclesToUpdate = (uint)(timestamp - _lastUpdate);
            _lastUpdate = timestamp;

            Update(cyclesToUpdate);
        }

        // Run this component for the required number of cycles
        public abstract void Update(uint cycleCount);
    }
}
