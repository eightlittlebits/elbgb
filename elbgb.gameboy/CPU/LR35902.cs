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

		private void AddMachineCycles(int cycleCount)
		{
			_timestamp += (ulong)(4 * cycleCount);
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

		// Push a byte on to the stack, decrement SP then write to memory
		private void PushByte(byte value)
		{
			WriteByte(--_r.SP, value);
		}

		// Pop a byte from the stack, read from memory then increment SP
		private byte PopByte()
		{
			return ReadByte(_r.SP++);
		}

		// Push a word onto the stack, high byte pushed first
		private void PushWord(ushort value)
		{
			PushByte((byte)(value >> 8));
			PushByte((byte)value);
		}

		// Pop a word from the stack, low byte popped first
		private ushort PopWord()
		{
			byte lo = PopByte();
			byte hi = PopByte();

			return (ushort)(hi << 8 | lo);
		}

		// calculate the carry flags based on the two addends and the result
		// based on http://stackoverflow.com/a/8037485 by Alexey Frunze
		private static Registers.Flags CalculateCarryFlags(byte a, byte b, int result)
		{
			// given bit n of the binary sum is calculated as a(n) XOR b(n) XOR carry-in we
			// can retrieve the values of the carry-in for each bit with a XOR b XOR result
			// with the carry-in values the carry in to bit 4 is the carry out from bit 3 
			// which is our half carry and the carry in to bit 8 is the carry out from bit
			// 7 which is our carry flag
			var carryIn = a ^ b ^ result;

			// now that we have our relevant flags we can shift them into the correct bit 
			// locations for the flags register, the carry flag right 4 from bit 8 into bit
			// 4 and the half carry left 1 from but 4 into bit 5
			return (Registers.Flags)(((carryIn & 0x100) >> 4) | (carryIn & 0x10) << 1);
		}

		private static Registers.Flags CalculateCarryFlags(ushort a, ushort b, int result)
		{
			var carryIn = (a ^ b ^ result) >> 8;

			return (Registers.Flags)(((carryIn & 0x100) >> 4) | (carryIn & 0x10) << 1);
		}

		public void ExecuteSingleInstruction()
		{
			byte opcode = ReadByte(_r.PC++);

			switch (opcode)
			{
				case 0x00: return; // NOP

				#region 8-bit transfer and input/output instructions

				// load contents of register r' into register r
				case 0x7F: _r.A = _r.A; break; // LD A,A
				case 0x78: _r.A = _r.B; break; // LD A,B
				case 0x79: _r.A = _r.C; break; // LD A,C
				case 0x7A: _r.A = _r.D; break; // LD A,D
				case 0x7B: _r.A = _r.E; break; // LD A,E
				case 0x7C: _r.A = _r.H; break; // LD A,H
				case 0x7D: _r.A = _r.L; break; // LD A,L
				case 0x47: _r.B = _r.A; break; // LD B,A
				case 0x40: _r.B = _r.B; break; // LD B,B
				case 0x41: _r.B = _r.C; break; // LD B,C
				case 0x42: _r.B = _r.D; break; // LD B,D
				case 0x43: _r.B = _r.E; break; // LD B,E
				case 0x44: _r.B = _r.H; break; // LD B,H
				case 0x45: _r.B = _r.L; break; // LD B,L
				case 0x4F: _r.C = _r.A; break; // LD C,A
				case 0x48: _r.C = _r.B; break; // LD C,B
				case 0x49: _r.C = _r.C; break; // LD C,C
				case 0x4A: _r.C = _r.D; break; // LD C,D
				case 0x4B: _r.C = _r.E; break; // LD C,E
				case 0x4C: _r.C = _r.H; break; // LD C,H
				case 0x4D: _r.C = _r.L; break; // LD C,L
				case 0x57: _r.D = _r.A; break; // LD D,A
				case 0x50: _r.D = _r.B; break; // LD D,B
				case 0x51: _r.D = _r.C; break; // LD D,C
				case 0x52: _r.D = _r.D; break; // LD D,D
				case 0x53: _r.D = _r.E; break; // LD D,E
				case 0x54: _r.D = _r.H; break; // LD D,H
				case 0x55: _r.D = _r.L; break; // LD D,L
				case 0x5F: _r.E = _r.A; break; // LD E,A
				case 0x58: _r.E = _r.B; break; // LD E,B
				case 0x59: _r.E = _r.C; break; // LD E,C
				case 0x5A: _r.E = _r.D; break; // LD E,D
				case 0x5B: _r.E = _r.E; break; // LD E,E
				case 0x5C: _r.E = _r.H; break; // LD E,H
				case 0x5D: _r.E = _r.L; break; // LD E,L
				case 0x67: _r.H = _r.A; break; // LD H,A
				case 0x60: _r.H = _r.B; break; // LD H,B
				case 0x61: _r.H = _r.C; break; // LD H,C
				case 0x62: _r.H = _r.D; break; // LD H,D
				case 0x63: _r.H = _r.E; break; // LD H,E
				case 0x64: _r.H = _r.H; break; // LD H,H
				case 0x65: _r.H = _r.L; break; // LD H,L
				case 0x6F: _r.L = _r.A; break; // LD L,A
				case 0x68: _r.L = _r.B; break; // LD L,B
				case 0x69: _r.L = _r.C; break; // LD L,C
				case 0x6A: _r.L = _r.D; break; // LD L,D
				case 0x6B: _r.L = _r.E; break; // LD L,E
				case 0x6C: _r.L = _r.H; break; // LD L,H
				case 0x6D: _r.L = _r.L; break; // LD L,L

				// load 8-bit immediate data n into register r
				case 0x3E: _r.A = ReadByte(_r.PC++); break; // LD A,n
				case 0x06: _r.B = ReadByte(_r.PC++); break; // LD B,n
				case 0x0E: _r.C = ReadByte(_r.PC++); break; // LD C,n
				case 0x16: _r.D = ReadByte(_r.PC++); break; // LD D,n
				case 0x1E: _r.E = ReadByte(_r.PC++); break; // LD E,n
				case 0x26: _r.H = ReadByte(_r.PC++); break; // LD H,n
				case 0x2E: _r.L = ReadByte(_r.PC++); break; // LD L,n

				// load the contents of memory (8 bits) specified by register pair HL into register r
				case 0x7E: _r.A = ReadByte(_r.HL); break; // LD A,(HL)
				case 0x46: _r.B = ReadByte(_r.HL); break; // LD B,(HL)
				case 0x4E: _r.C = ReadByte(_r.HL); break; // LD C,(HL)
				case 0x56: _r.D = ReadByte(_r.HL); break; // LD D,(HL)
				case 0x5E: _r.E = ReadByte(_r.HL); break; // LD E,(HL)
				case 0x66: _r.H = ReadByte(_r.HL); break; // LD H,(HL)
				case 0x6E: _r.L = ReadByte(_r.HL); break; // LD L,(HL)

				// stores the content of register r in memory specififed by register pair HL
				case 0x77: WriteByte(_r.HL, _r.A); break; // LD (HL),A
				case 0x70: WriteByte(_r.HL, _r.B); break; // LD (HL),B
				case 0x71: WriteByte(_r.HL, _r.C); break; // LD (HL),C
				case 0x72: WriteByte(_r.HL, _r.D); break; // LD (HL),D
				case 0x73: WriteByte(_r.HL, _r.E); break; // LD (HL),E
				case 0x74: WriteByte(_r.HL, _r.H); break; // LD (HL),H
				case 0x75: WriteByte(_r.HL, _r.L); break; // LD (HL),L

				// loads 8-bit immediate data n into memory specified by register pair HL
				case 0x36: WriteByte(_r.HL, ReadByte(_r.PC++)); break; // LD (HL),n

				// load the contents of memory (8 bits) specified by register pair into register A
				case 0x0A: _r.A = ReadByte(_r.BC); break; // LD A,(BC)
				case 0x1A: _r.A = ReadByte(_r.DE); break; // LD A,(DE)

				// load/store high memory, indexed on C
				case 0xF2: _r.A = ReadByte((ushort)(0xFF00 + _r.C)); break; // LD A,($FF00+C)
				case 0xE2: WriteByte((ushort)(0xFF00 + _r.C), _r.A); break; // LD ($FF00+C),A

				// load/store high memory, indexed on immediate
				case 0xF0: _r.A = ReadByte((ushort)(0xFF00 + ReadByte(_r.PC++))); break; // LD A,($FF00+n)
				case 0xE0: WriteByte((ushort)(0xFF00 + ReadByte(_r.PC++)), _r.A); break; // LD ($FF00+n),A

				// load/store register A from/to internal RAM or register specified by 16-bit immediate
				case 0xFA: _r.A = ReadByte(ReadWord(_r.PC)); _r.PC += 2; break; // LD A,(nn)
				case 0xEA: WriteByte(ReadWord(_r.PC), _r.A); _r.PC += 2; break; // LD (nn),A

				// load the contents of memory (8 bits) specified by register pair HL into register A
				// and simultaneously increment/decrement HL
				case 0x2A: _r.A = ReadByte(_r.HL++); break; // LD A,(HL+)
				case 0x3A: _r.A = ReadByte(_r.HL--); break; // LD A,(HL-)

				// Store the contents of register A in the memory specified by register pair 
				case 0x02: WriteByte(_r.BC, _r.A); break; // LD (BC),A
				case 0x12: WriteByte(_r.DE, _r.A); break; // LD (DE),A

				// store the contents of register A in the memoryspecified by register pair HL
				// and simultaneously increment/decrement HL
				case 0x22: WriteByte(_r.HL++, _r.A); break; // LD (HL+),A
				case 0x32: WriteByte(_r.HL--, _r.A); break; // LD (HL-),A

				#endregion

				#region 16-bit transfer instructions

				// load 2 bytes of immediate data to register pair
				case 0x01: _r.BC = ReadWord(_r.PC); _r.PC += 2; break; // LD BC,nn
				case 0x11: _r.DE = ReadWord(_r.PC); _r.PC += 2; break; // LD DE,nn
				case 0x21: _r.HL = ReadWord(_r.PC); _r.PC += 2; break; // LD HL,nn
				case 0x31: _r.SP = ReadWord(_r.PC); _r.PC += 2; break; // LD SP,nn

				// load the contents of register pair HL in stack pointer SP
				case 0xF9: _r.SP = _r.HL; AddMachineCycles(1); break; // LD SP,HL

				// push contents of register pair onto the stack
				case 0xC5: AddMachineCycles(1); PushWord(_r.BC); break; // PUSH BC
				case 0xD5: AddMachineCycles(1); PushWord(_r.DE); break; // PUSH DE
				case 0xE5: AddMachineCycles(1); PushWord(_r.HL); break; // PUSH HL
				case 0xF5: AddMachineCycles(1); PushWord(_r.AF); break; // PUSH AF

				// pop contents of stack into register pair
				case 0xC1: _r.BC = PopWord(); break; // POP BC
				case 0xD1: _r.DE = PopWord(); break; // POP DE
				case 0xE1: _r.HL = PopWord(); break; // POP HL
				case 0xF1: _r.AF = PopWord(); break; // POP AF

				// the 8-bit operand e is added to SP and the result stored in HL
				case 0xF8: // LDHL SP,e
					{
						byte e = ReadByte(_r.PC++);
						var result = _r.SP + e;

						_r.HL = (ushort)result;
						_r.F = CalculateCarryFlags(_r.SP, e, result);

						AddMachineCycles(1);
					} break;

				// store the lower byte of SP at address nn specified by the 16-bit immediate operand nn
				// and the upper byte of SP and address nn + 1
				case 0x08: // LD (nn), SP
					{
						ushort address = ReadWord(_r.PC);
						_r.PC += 2;

						WriteByte(address, (byte)(_r.SP));
						WriteByte((ushort)(address + 1), (byte)(_r.SP >> 8));
					} break;

				#endregion

				#region 8-bit arithmetic and logical operation instructions

				case 0xAF: _r.A = Xor8Bit(_r.A, _r.A); break;				// XOR A
				case 0xA8: _r.A = Xor8Bit(_r.A, _r.B); break;				// XOR B
				case 0xA9: _r.A = Xor8Bit(_r.A, _r.C); break;				// XOR C
				case 0xAA: _r.A = Xor8Bit(_r.A, _r.D); break;				// XOR D
				case 0xAB: _r.A = Xor8Bit(_r.A, _r.E); break;				// XOR E
				case 0xAC: _r.A = Xor8Bit(_r.A, _r.H); break;				// XOR H
				case 0xAD: _r.A = Xor8Bit(_r.A, _r.L); break;				// XOR L
				case 0xAE: _r.A = Xor8Bit(_r.A, ReadByte(_r.HL)); break;	// XOR (HL)
				case 0xEE: _r.A = Xor8Bit(_r.A, ReadByte(_r.PC++)); break;	// XOR n

				#endregion

				#region 16-bit arithmetic operation instructions

				#endregion

				// extended opcodes are prefixed with OxCB, read next byte and run opcode 
				case 0xCB: ExecuteExtendedOpcode(ReadByte(_r.PC++)); break;

				#region jump instructions

				case 0x20: if (!_r.F.HasFlag(Registers.Flags.Z)) { _r.PC = (ushort)(_r.PC + (sbyte)ReadByte(_r.PC++)); _r.PC++; } else { _r.PC++; } break; // JR NZ,n

				#endregion

				default:
					throw new NotImplementedException(string.Format("Invalid opcode 0x{0:X2} at {1:X4}", opcode, _r.PC - 1));
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
			byte result = (byte)(b1 ^ b2);

			// clear flags
			_r.F = 0;

			// set zero flag if required
			if (result == 0)
				_r.F |= Registers.Flags.Z;

			return result;
		}

		// test if bit bit is set in byte reg, preserve carry flag, set half carry, set zero if bit not set
		private void Bit(byte reg, int bit)
		{
			_r.F &= Registers.Flags.C;
			_r.F |= Registers.Flags.H;

			// set zero flag if bit N of reg is not set
			if ((reg & (1 << bit)) == 0)
			{
				_r.F |= Registers.Flags.Z;
			}
		}
	}
}
