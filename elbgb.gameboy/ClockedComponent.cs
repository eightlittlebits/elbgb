using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	public abstract class ClockedComponent
	{
		protected GameBoy _gb;
		protected ulong _lastUpdate;

		public ClockedComponent(GameBoy gameBoy)
		{
			_gb = gameBoy;
		}

		// Synchronise this component with the current system clock 
		public void SynchroniseWithSystemClock()
		{
			ulong cyclesToUpdate = _gb.Clock.Timestamp - _lastUpdate;

			_lastUpdate = _gb.Clock.Timestamp;

			Update(cyclesToUpdate);
		}

		// Run this component for the required number of cycles
		public abstract void Update(ulong cycleCount);
	}
}
