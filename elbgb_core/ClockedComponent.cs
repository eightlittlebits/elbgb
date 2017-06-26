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
        private SystemClock _clock;
        protected ulong _lastUpdate;

        public ClockedComponent(GameBoy gameBoy)
        {
            _gb = gameBoy;
            _clock = gameBoy.Clock;
        }

        public void SynchroniseWithSystemClock()
        {
            ulong timestamp = _clock.Timestamp;
            uint cyclesToUpdate = (uint)(timestamp - _lastUpdate);
            _lastUpdate = timestamp;

            Update(cyclesToUpdate);
        }

        // Run this component for the required number of cycles
        public abstract void Update(uint cycleCount);
    }
}
