using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
	public struct GBCoreInput
	{
		public bool Up;
		public bool Down;
		public bool Left;
		public bool Right;

		public bool A;
		public bool B;

		public bool Start;
		public bool Select;

		internal bool ButtonPressed(GBCoreInput previous)
		{
			// check each button, if any of them were not pressed previously and they are 
			// now we return true
			if ((!previous.Up && Up)
				|| (!previous.Down && Down)
				|| (!previous.Left && Left)
				|| (!previous.Right && Right)
				|| (!previous.A && A)
				|| (!previous.B && B)
				|| (!previous.Start && Start)
				|| (!previous.Select && Select))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
