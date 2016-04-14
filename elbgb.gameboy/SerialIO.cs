using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gbcore
{
	public class SerialIO : ClockedComponent
	{
		public SerialIO(GameBoy gameBoy)
			: base(gameBoy)
		{

		}

		public override void Update(ulong cycleCount)
		{
			return;
		}
	}
}
