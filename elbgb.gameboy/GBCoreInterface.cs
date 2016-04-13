using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gbcore
{
	public struct GBCoreInterface
	{
		public Action<byte[]> PresentScreenData;
		//public Func<GBCoreInput> PollInput;
	}
}
