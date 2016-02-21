using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gameboy.Memory;

namespace elbgb.gameboy.CPU
{
	class LR35902
	{
		private MMU _mmu;
		private Registers _r;

		private ulong _timestamp;

		public ulong Timestamp { get { return _timestamp; } }

		public LR35902(MMU mmu)
		{
			_mmu = mmu;
		}
		
		// Wrap the MMU ReadByte to handle the timing updates
		private byte ReadByte(ushort address)
		{
			byte value = _mmu.ReadByte(address);
			_timestamp += 4;

			return value;
		}

		// Wrap the MMU ReadWord to handle the timing updates
		private ushort ReadWord(ushort address)
		{
			byte lo = _mmu.ReadByte(address);
			_timestamp += 4;

			byte hi = _mmu.ReadByte((ushort)(address + 1));
			_timestamp += 4;

			return (ushort)(hi << 8 | lo);
		}

		// Wrap the MMU WriteByte to handle the timing updates
		private void WriteByte(ushort address, byte value)
		{
			_mmu.WriteByte(address, value);
			_timestamp += 4;
		}

		public void ExecuteSingleInstruction()
		{
			byte opcode = ReadByte(_r.PC++);

			switch (opcode)
			{
				case 0x00: return; // NOP

				// Write A to (HL) and inc/dec
				case 0x22: WriteByte(_r.HL++, _r.A); break; // LD (HL-),A
				case 0x32: WriteByte(_r.HL--, _r.A); break; // LD (HL-),A

				// Load 16 bit immediate
				case 0x21: _r.HL = ReadWord(_r.PC); _r.PC += 2; break; // LD HL,nn
				case 0x31: _r.SP = ReadWord(_r.PC); _r.PC += 2; break; // LD SP,nn

				// 8 bit ALU
				case 0xAF: _r.A = Xor8Bit(_r.A, _r.A); break;				// XOR A
				case 0xA8: _r.A = Xor8Bit(_r.A, _r.B); break;				// XOR B
				case 0xA9: _r.A = Xor8Bit(_r.A, _r.C); break;				// XOR C
				case 0xAA: _r.A = Xor8Bit(_r.A, _r.D); break;				// XOR D
				case 0xAB: _r.A = Xor8Bit(_r.A, _r.E); break;				// XOR E
				case 0xAC: _r.A = Xor8Bit(_r.A, _r.H); break;				// XOR H
				case 0xAD: _r.A = Xor8Bit(_r.A, _r.L); break;				// XOR L
				case 0xAE: _r.A = Xor8Bit(_r.A, ReadByte(_r.HL)); break;	// XOR (HL)
				case 0xEE: _r.A = Xor8Bit(_r.A, ReadByte(_r.PC++)); break;	// XOR n

				// extended opcodes are prefixed with OxCB, read next byte and run opcode 
				case 0xCB: ExecuteExtendedOpcode(ReadByte(_r.PC++)); break; 

				// jump instructions
				case 0x20: if (!_r.F.HasFlag(Registers.Flags.Z)) { _r.PC = (ushort)(_r.PC + (sbyte)ReadByte(_r.PC++)); _r.PC++; } else { _r.PC++; } break; // JR NZ,n

				default:
					throw new NotImplementedException(string.Format("Invalid opcode 0x{0:X2} at {1:X4}", opcode, _r.PC-1));
			}
		}

		private void ExecuteExtendedOpcode(byte opcode)
		{
			switch (opcode)
			{
				case 0x7C: Bit(_r.H, 7); break;

				default:
					throw new NotImplementedException(string.Format("Invalid opcode 0x{0:X4} at {1:X4}", 0xCB00 | opcode, _r.PC - 2));
			}
		}

		private byte Xor8Bit(byte b1, byte b2)
		{
			byte value = (byte)(b1 ^ b2);

			// clear flags
			_r.F = 0;

			// set zero flag if required
			if (value == 0)
				_r.F |= Registers.Flags.Z;

			return value;
		}

		private void Bit(byte reg, int bit)
		{
			// carry flag unaffected, preserve state
			_r.F &= Registers.Flags.C;

			// half carry flag set
			_r.F |= Registers.Flags.H;

			// set zero flag if bit N of reg is not set
			if ((reg & (1 << bit)) == 0)
			{
				_r.F |= Registers.Flags.Z;
			}
		}
	}
}
