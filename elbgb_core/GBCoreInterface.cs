using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core
{
	public struct GBCoreInterface
	{
		public Action<byte> SerialTransferComplete;
		public Action<byte[]> PresentScreenData;
		
		public Func<GBCoreInput> PollInput;
	}
}
