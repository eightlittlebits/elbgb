using System;
namespace elbgb.gameboy.Memory
{
	interface ICartridge
	{
		byte ReadByte(ushort address);
		void WriteByte(ushort address, byte value);
	}
}
