﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb.gameboy
{
	interface IMemoryMappedComponent
	{
		byte ReadByte(ushort address);
		void WriteByte(ushort address, byte value);
	}
}
