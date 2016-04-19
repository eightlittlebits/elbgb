using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elbgb.gbcore.Memory;

namespace elbgb.gbcore.CPU
{
	public class LR35902
	{
		private GameBoy _gb;
		private Registers _r;

		private bool _halted;

		public LR35902(GameBoy gameBoy)
		{
			_gb = gameBoy;
		}

		// Wrap the MMU ReadByte to handle the timing updates
		private byte ReadByte(ushort address)
		{
			byte value = _gb.MMU.ReadByte(address);
			_gb.Clock.AddMachineCycles(1);

			return value;
		}

		// Wrap the MMU ReadWord to handle the timing updates
		private ushort ReadWord(ushort address)
		{
			byte lo = ReadByte(address);
			byte hi = ReadByte((ushort)(address + 1));

			return (ushort)(hi << 8 | lo);
		}

		// Wrap the MMU WriteByte to handle the timing updates
		private void WriteByte(ushort address, byte value)
		{
			_gb.MMU.WriteByte(address, value);
			_gb.Clock.AddMachineCycles(1);
		}

		#region stack handling

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

		#endregion

		internal void ProcessInterrupts()
		{
			byte interruptFlag = _gb.MMU.ReadByte(MMU.Registers.IF);
			byte interruptEnable = _gb.MMU.ReadByte(MMU.Registers.IE);

			// bitwise and of the enable flag and the interrupt flag will only leave any bits set
			// if the interrupt has been requested and the interrupt is enabled
			int pendingInterrupt = interruptFlag & interruptEnable;

			// pending interrupt to service?
			if (pendingInterrupt != 0)
			{
				// clear the halt flag on interrupt
				_halted = false;

				// interrupts enabled?
				if (_r.IME)
				{
					// check each bit in the pending interrupt to find out which needs to be processed
					for (int i = 0; i < 5; i++)
					{
						int interrupt = 1 << i;

						if ((pendingInterrupt & interrupt) == interrupt)
						{
							// timing information from
							// https://github.com/Gekkio/mooneye-gb/blob/master/docs/accuracy.markdown
							// the above has been tested on a real DMG, 3 wait cycles followed by
							// the push of the PC

							// 3 wait cycles
							_gb.Clock.AddMachineCycles(3);

							// disable interrupts
							_r.IME = false;

							// clear the interrupt flag
							_gb.MMU.WriteByte(MMU.Registers.IF, (byte)(interruptFlag & ~interrupt));

							// push current PC to stack - 2 cycles
							PushWord(_r.PC);

							// jump to interrupt address based on interrupt
							switch (i)
							{
								case 0: _r.PC = 0x0040; break; // Vertical Blank
								case 1: _r.PC = 0x0048; break; // LCDC Status
								case 2: _r.PC = 0x0050; break; // Timer Overflow
								case 3: _r.PC = 0x0058; break; // Serial Transfer Complete
								case 4: _r.PC = 0x0060; break; // User Input
							}

							// interrupt processed, break from loop
							break;
						}
					}
				}
			}
		}

		public void ExecuteSingleInstruction()
		{
			if (_halted)
			{
				_gb.Clock.AddMachineCycles(1);
				return;
			}
			
			byte opcode = ReadByte(_r.PC++);

			switch (opcode)
			{
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
				case 0xF9: _r.SP = _r.HL; _gb.Clock.AddMachineCycles(1); break; // LD SP,HL

				// push contents of register pair onto the stack
				case 0xC5: _gb.Clock.AddMachineCycles(1); PushWord(_r.BC); break; // PUSH BC
				case 0xD5: _gb.Clock.AddMachineCycles(1); PushWord(_r.DE); break; // PUSH DE
				case 0xE5: _gb.Clock.AddMachineCycles(1); PushWord(_r.HL); break; // PUSH HL
				case 0xF5: _gb.Clock.AddMachineCycles(1); PushWord(_r.AF); break; // PUSH AF

				// pop contents of stack into register pair
				case 0xC1: _r.BC = PopWord(); break; // POP BC
				case 0xD1: _r.DE = PopWord(); break; // POP DE
				case 0xE1: _r.HL = PopWord(); break; // POP HL
				case 0xF1: _r.AF = PopWord(); break; // POP AF

				// the 8-bit operand e is added to SP and the result stored in HL
				// the documentation appears to be incorrect with respect to the flags
				// the flag results are based on the lower byte and not the upper byte
				case 0xF8: // LDHL SP,e
					{
						sbyte e = (sbyte)ReadByte(_r.PC++);

						_r.F = StatusFlags.Clear;

						if (((_r.SP & 0xFF) + (e & 0xFF)) > 0xFF)
							_r.F |= StatusFlags.C;

						if (((_r.SP & 0x0F) + (e & 0x0F)) > 0x0F)
							_r.F |= StatusFlags.H;

						_r.HL = (ushort)(_r.SP + e);

						_gb.Clock.AddMachineCycles(1);
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

				case 0x87: _r.A = Add8Bit(_r.A, _r.A); break;						// ADD A
				case 0x80: _r.A = Add8Bit(_r.A, _r.B); break;						// ADD B
				case 0x81: _r.A = Add8Bit(_r.A, _r.C); break;						// ADD C
				case 0x82: _r.A = Add8Bit(_r.A, _r.D); break;						// ADD D
				case 0x83: _r.A = Add8Bit(_r.A, _r.E); break;						// ADD E
				case 0x84: _r.A = Add8Bit(_r.A, _r.H); break;						// ADD H
				case 0x85: _r.A = Add8Bit(_r.A, _r.L); break;						// ADD L
				case 0x86: _r.A = Add8Bit(_r.A, ReadByte(_r.HL)); break;			// ADD (HL)
				case 0xC6: _r.A = Add8Bit(_r.A, ReadByte(_r.PC++)); break;			// ADD n

				case 0x8F: _r.A = AddWithCarry8Bit(_r.A, _r.A); break;				// ADC A
				case 0x88: _r.A = AddWithCarry8Bit(_r.A, _r.B); break;				// ADC B
				case 0x89: _r.A = AddWithCarry8Bit(_r.A, _r.C); break;				// ADC C
				case 0x8A: _r.A = AddWithCarry8Bit(_r.A, _r.D); break;				// ADC D
				case 0x8B: _r.A = AddWithCarry8Bit(_r.A, _r.E); break;				// ADC E
				case 0x8C: _r.A = AddWithCarry8Bit(_r.A, _r.H); break;				// ADC H
				case 0x8D: _r.A = AddWithCarry8Bit(_r.A, _r.L); break;				// ADC L
				case 0x8E: _r.A = AddWithCarry8Bit(_r.A, ReadByte(_r.HL)); break;	// ADC (HL)
				case 0xCE: _r.A = AddWithCarry8Bit(_r.A, ReadByte(_r.PC++)); break;	// ADC n

				case 0x97: _r.A = Sub8Bit(_r.A, _r.A); break;						// SUB A
				case 0x90: _r.A = Sub8Bit(_r.A, _r.B); break;						// SUB B
				case 0x91: _r.A = Sub8Bit(_r.A, _r.C); break;						// SUB C
				case 0x92: _r.A = Sub8Bit(_r.A, _r.D); break;						// SUB D
				case 0x93: _r.A = Sub8Bit(_r.A, _r.E); break;						// SUB E
				case 0x94: _r.A = Sub8Bit(_r.A, _r.H); break;						// SUB H
				case 0x95: _r.A = Sub8Bit(_r.A, _r.L); break;						// SUB L
				case 0x96: _r.A = Sub8Bit(_r.A, ReadByte(_r.HL)); break;			// SUB (HL)
				case 0xD6: _r.A = Sub8Bit(_r.A, ReadByte(_r.PC++)); break;			// SUB n

				case 0x9F: _r.A = SubWithCarry8Bit(_r.A, _r.A); break;				// SBC A
				case 0x98: _r.A = SubWithCarry8Bit(_r.A, _r.B); break;				// SBC B
				case 0x99: _r.A = SubWithCarry8Bit(_r.A, _r.C); break;				// SBC C
				case 0x9A: _r.A = SubWithCarry8Bit(_r.A, _r.D); break;				// SBC D
				case 0x9B: _r.A = SubWithCarry8Bit(_r.A, _r.E); break;				// SBC E
				case 0x9C: _r.A = SubWithCarry8Bit(_r.A, _r.H); break;				// SBC H
				case 0x9D: _r.A = SubWithCarry8Bit(_r.A, _r.L); break;				// SBC L
				case 0x9E: _r.A = SubWithCarry8Bit(_r.A, ReadByte(_r.HL)); break;	// SBC (HL)
				case 0xDE: _r.A = SubWithCarry8Bit(_r.A, ReadByte(_r.PC++)); break;	// SBC n

				case 0xA7: _r.A = And8Bit(_r.A, _r.A); break;						// AND A
				case 0xA0: _r.A = And8Bit(_r.A, _r.B); break;						// AND B
				case 0xA1: _r.A = And8Bit(_r.A, _r.C); break;						// AND C
				case 0xA2: _r.A = And8Bit(_r.A, _r.D); break;						// AND D
				case 0xA3: _r.A = And8Bit(_r.A, _r.E); break;						// AND E
				case 0xA4: _r.A = And8Bit(_r.A, _r.H); break;						// AND H
				case 0xA5: _r.A = And8Bit(_r.A, _r.L); break;						// AND L
				case 0xA6: _r.A = And8Bit(_r.A, ReadByte(_r.HL)); break;			// AND (HL)
				case 0xE6: _r.A = And8Bit(_r.A, ReadByte(_r.PC++)); break;			// AND n

				case 0xB7: _r.A = Or8Bit(_r.A, _r.A); break;						// OR A
				case 0xB0: _r.A = Or8Bit(_r.A, _r.B); break;						// OR B
				case 0xB1: _r.A = Or8Bit(_r.A, _r.C); break;						// OR C
				case 0xB2: _r.A = Or8Bit(_r.A, _r.D); break;						// OR D
				case 0xB3: _r.A = Or8Bit(_r.A, _r.E); break;						// OR E
				case 0xB4: _r.A = Or8Bit(_r.A, _r.H); break;						// OR H
				case 0xB5: _r.A = Or8Bit(_r.A, _r.L); break;						// OR L
				case 0xB6: _r.A = Or8Bit(_r.A, ReadByte(_r.HL)); break;				// OR (HL)
				case 0xF6: _r.A = Or8Bit(_r.A, ReadByte(_r.PC++)); break;			// OR n

				case 0xAF: _r.A = Xor8Bit(_r.A, _r.A); break;						// XOR A
				case 0xA8: _r.A = Xor8Bit(_r.A, _r.B); break;						// XOR B
				case 0xA9: _r.A = Xor8Bit(_r.A, _r.C); break;						// XOR C
				case 0xAA: _r.A = Xor8Bit(_r.A, _r.D); break;						// XOR D
				case 0xAB: _r.A = Xor8Bit(_r.A, _r.E); break;						// XOR E
				case 0xAC: _r.A = Xor8Bit(_r.A, _r.H); break;						// XOR H
				case 0xAD: _r.A = Xor8Bit(_r.A, _r.L); break;						// XOR L
				case 0xAE: _r.A = Xor8Bit(_r.A, ReadByte(_r.HL)); break;			// XOR (HL)
				case 0xEE: _r.A = Xor8Bit(_r.A, ReadByte(_r.PC++)); break;			// XOR n

				case 0xBF: Compare8Bit(_r.A, _r.A); break;							// CP A
				case 0xB8: Compare8Bit(_r.A, _r.B); break;							// CP B
				case 0xB9: Compare8Bit(_r.A, _r.C); break;							// CP C
				case 0xBA: Compare8Bit(_r.A, _r.D); break;							// CP D
				case 0xBB: Compare8Bit(_r.A, _r.E); break;							// CP E
				case 0xBC: Compare8Bit(_r.A, _r.H); break;							// CP H
				case 0xBD: Compare8Bit(_r.A, _r.L); break;							// CP L
				case 0xBE: Compare8Bit(_r.A, ReadByte(_r.HL)); break;				// CP (HL)
				case 0xFE: Compare8Bit(_r.A, ReadByte(_r.PC++)); break;				// CP n

				case 0x3C: _r.A = Inc8Bit(_r.A); break;								// INC A
				case 0x04: _r.B = Inc8Bit(_r.B); break;								// INC B
				case 0x0C: _r.C = Inc8Bit(_r.C); break;								// INC C
				case 0x14: _r.D = Inc8Bit(_r.D); break;								// INC D
				case 0x1C: _r.E = Inc8Bit(_r.E); break;								// INC E
				case 0x24: _r.H = Inc8Bit(_r.H); break;								// INC H
				case 0x2C: _r.L = Inc8Bit(_r.L); break;								// INC L
				case 0x34: WriteByte(_r.HL, Inc8Bit(ReadByte(_r.HL))); break;		// INC (HL)

				case 0x3D: _r.A = Dec8Bit(_r.A); break;								// DEC A
				case 0x05: _r.B = Dec8Bit(_r.B); break;								// DEC B
				case 0x0D: _r.C = Dec8Bit(_r.C); break;								// DEC C
				case 0x15: _r.D = Dec8Bit(_r.D); break;								// DEC D
				case 0x1D: _r.E = Dec8Bit(_r.E); break;								// DEC E
				case 0x25: _r.H = Dec8Bit(_r.H); break;								// DEC H
				case 0x2D: _r.L = Dec8Bit(_r.L); break;								// DEC L
				case 0x35: WriteByte(_r.HL, Dec8Bit(ReadByte(_r.HL))); break;		// DEC (HL)

				#endregion

				#region 16-bit arithmetic operation instructions

				case 0x09: _r.HL = Add16Bit(_r.HL, _r.BC); _gb.Clock.AddMachineCycles(1); break; // ADD HL,BC
				case 0x19: _r.HL = Add16Bit(_r.HL, _r.DE); _gb.Clock.AddMachineCycles(1); break; // ADD HL,DE
				case 0x29: _r.HL = Add16Bit(_r.HL, _r.HL); _gb.Clock.AddMachineCycles(1); break; // ADD HL,HL
				case 0x39: _r.HL = Add16Bit(_r.HL, _r.SP); _gb.Clock.AddMachineCycles(1); break; // ADD HL,SP

				case 0xE8: // ADD SP,e
					{
						sbyte e = (sbyte)ReadByte(_r.PC++);

						_r.F = StatusFlags.Clear;

						if (((_r.SP & 0xFF) + (e & 0xFF)) > 0xFF)
							_r.F |= StatusFlags.C;

						if (((_r.SP & 0x0F) + (e & 0x0F)) > 0x0F)
							_r.F |= StatusFlags.H;

						_r.SP = (ushort)(_r.SP + e);

						_gb.Clock.AddMachineCycles(2);
					} break;

				case 0x03: _r.BC++; _gb.Clock.AddMachineCycles(1); break; // INC BC
				case 0x13: _r.DE++; _gb.Clock.AddMachineCycles(1); break; // INC DE
				case 0x23: _r.HL++; _gb.Clock.AddMachineCycles(1); break; // INC HL
				case 0x33: _r.SP++; _gb.Clock.AddMachineCycles(1); break; // INC SP

				case 0x0B: _r.BC--; _gb.Clock.AddMachineCycles(1); break; // DEC BC
				case 0x1B: _r.DE--; _gb.Clock.AddMachineCycles(1); break; // DEC DE
				case 0x2B: _r.HL--; _gb.Clock.AddMachineCycles(1); break; // DEC HL
				case 0x3B: _r.SP--; _gb.Clock.AddMachineCycles(1); break; // DEC SP

				#endregion

				#region rotate, shift and bit instructions

				// the same rotate methods are used for the extended opcodes, these do not set the Z flag
				case 0x07: _r.A = RotateLeft(_r.A); _r.F &= ~StatusFlags.Z; break; // RLCA
				case 0x17: _r.A = RotateLeftThroughCarry(_r.A); _r.F &= ~StatusFlags.Z; break; // RLA

				case 0x0F: _r.A = RotateRight(_r.A); _r.F &= ~StatusFlags.Z; break; // RRCA
				case 0x1F: _r.A = RotateRightThroughCarry(_r.A); _r.F &= ~StatusFlags.Z; break; // RRA

				// extended opcodes are prefixed with OxCB, read next byte and run opcode 
				case 0xCB: ExecuteExtendedOpcode(ReadByte(_r.PC++)); break;

				#endregion

				#region jump instructions

				// loads the operand nn into the program counter (PC)
				case 0xC3: JumpImmediate(); break; // JP nn

				// loads the operand nn into the PC if condition cc and the status flag match
				case 0xC2: JumpImmediate(!_r.F.FlagSet(StatusFlags.Z)); break; // JP NZ,nn
				case 0xCA: JumpImmediate(_r.F.FlagSet(StatusFlags.Z)); break;  // JP Z,nn
				case 0xD2: JumpImmediate(!_r.F.FlagSet(StatusFlags.C)); break; // JP NC,nn
				case 0xDA: JumpImmediate(_r.F.FlagSet(StatusFlags.C)); break;  // JP C,nn

				// jumps -127 to +129 steps from current address
				case 0x18: JumpRelative(); break; // JR e

				// if condition and status flag match, jumps -127 to +129 steps from current address
				case 0x20: JumpRelative(!_r.F.FlagSet(StatusFlags.Z)); break; // JR NZ,n
				case 0x28: JumpRelative(_r.F.FlagSet(StatusFlags.Z)); break;  // JR Z,n
				case 0x30: JumpRelative(!_r.F.FlagSet(StatusFlags.C)); break; // JR NC,n
				case 0x38: JumpRelative(_r.F.FlagSet(StatusFlags.C)); break;  // JR C,n

				// loads the contents of register pair HL in program counter PC
				case 0xE9: _r.PC = _r.HL; break; // JP (HL)

				#endregion

				#region call and return instructions

				// push PC onto stack and replace PC with immmediate value nn
				case 0xCD: CallImmediate(); break; // CALL nn

				// if condition and status flag match, push PC onto stack and replace PC with immmediate value nn
				case 0xC4: CallImmediate(!_r.F.FlagSet(StatusFlags.Z)); break; // CALL NZ,n
				case 0xCC: CallImmediate(_r.F.FlagSet(StatusFlags.Z)); break;  // CALL Z,n
				case 0xD4: CallImmediate(!_r.F.FlagSet(StatusFlags.C)); break; // CALL NC,n
				case 0xDC: CallImmediate(_r.F.FlagSet(StatusFlags.C)); break;  // CALL C,n

				// pops from the stack the PC value pushed when the subroutine was called
				case 0xC9: Return(); break; // RET

				// pops from the stack the PC value pushed when the subroutine was called, renables interrupts
				case 0xD9: Return(); _r.IME = true; break; // RETI

				// if condition and status flag match, pops from the stack the PC value pushed when the subroutine was called
				case 0xC0: Return(!_r.F.FlagSet(StatusFlags.Z)); break; // RET NZ
				case 0xC8: Return(_r.F.FlagSet(StatusFlags.Z)); break;  // RET Z
				case 0xD0: Return(!_r.F.FlagSet(StatusFlags.C)); break; // RET NC
				case 0xD8: Return(_r.F.FlagSet(StatusFlags.C)); break;  // RET C

				// push PC onto stack and load the PC with the page 0 address provided by operand t
				// operand t is provided in bits 3,4 and 5 of the operand (11tt t111) so masking opcode 
				// with 0x38 (0011 1000) retrieves the zero page address
				case 0xC7: Reset(0x00); break; // RST 1
				case 0xCF: Reset(0x08); break; // RST 2
				case 0xD7: Reset(0x10); break; // RST 3
				case 0xDF: Reset(0x18); break; // RST 4
				case 0xE7: Reset(0x20); break; // RST 5
				case 0xEF: Reset(0x28); break; // RST 6
				case 0xF7: Reset(0x30); break; // RST 7
				case 0xFF: Reset(0x38); break; // RST 8

				#endregion

				#region general purpose arithmetic operations and CPU control instructions

				// adjust the result in the accumulator after a BCD addition or subtraction
				case 0x27: DecimalAdjustAccumulator(); break; // DAA

				// takes the ones complement of register a
				case 0x2F: _r.A = (byte)~_r.A; _r.F |= (StatusFlags.N | StatusFlags.H); break; // CPL

				// PC advances, no other effects
				case 0x00: break; // NOP

				// complement carry flag, resets HN and preserves Z
				case 0x3F: _r.F = (_r.F &= (StatusFlags.Z | StatusFlags.C)) ^ StatusFlags.C; break; // CCF

				// set carry flag, resets HN and preserves Z
				case 0x37: _r.F = (_r.F &= StatusFlags.Z) | StatusFlags.C; break; //SCF

				// disable/enable interrupts
				case 0xF3: _r.IME = false; break; // DI
				case 0xFB: _r.IME = true; break; // EI

				// stops execution until an interrupt occurs
				case 0x76: _halted = true; break; // HALT

				// TODO(david): Implement STOP
				case 0x10: _r.PC++; break; // STOP

				#endregion

				default:
					throw new NotImplementedException(string.Format("Invalid opcode 0x{0:X2} at 0x{1:X4}", opcode, _r.PC - 1));
			}
		}

		private void ExecuteExtendedOpcode(byte opcode)
		{
			switch (opcode)
			{
				#region rotate shift instruction handlers

				case 0x00: _r.B = RotateLeft(_r.B); break; // RLC B
				case 0x01: _r.C = RotateLeft(_r.C); break; // RLC C
				case 0x02: _r.D = RotateLeft(_r.D); break; // RLC D
				case 0x03: _r.E = RotateLeft(_r.E); break; // RLC E
				case 0x04: _r.H = RotateLeft(_r.H); break; // RLC H
				case 0x05: _r.L = RotateLeft(_r.L); break; // RLC L
				case 0x06: WriteByte(_r.HL, RotateLeft(ReadByte(_r.HL))); break; // RLC (HL)
				case 0x07: _r.A = RotateLeft(_r.A); break; // RLC A

				case 0x10: _r.B = RotateLeftThroughCarry(_r.B); break; // RL B
				case 0x11: _r.C = RotateLeftThroughCarry(_r.C); break; // RL C
				case 0x12: _r.D = RotateLeftThroughCarry(_r.D); break; // RL D
				case 0x13: _r.E = RotateLeftThroughCarry(_r.E); break; // RL E
				case 0x14: _r.H = RotateLeftThroughCarry(_r.H); break; // RL H
				case 0x15: _r.L = RotateLeftThroughCarry(_r.L); break; // RL L
				case 0x16: WriteByte(_r.HL, RotateLeftThroughCarry(ReadByte(_r.HL))); break; // RL (HL)
				case 0x17: _r.A = RotateLeftThroughCarry(_r.A); break; // RL A

				case 0x08: _r.B = RotateRight(_r.B); break; // RRC B
				case 0x09: _r.C = RotateRight(_r.C); break; // RRC C
				case 0x0A: _r.D = RotateRight(_r.D); break; // RRC D
				case 0x0B: _r.E = RotateRight(_r.E); break; // RRC E
				case 0x0C: _r.H = RotateRight(_r.H); break; // RRC H
				case 0x0D: _r.L = RotateRight(_r.L); break; // RRC L
				case 0x0E: WriteByte(_r.HL, RotateRight(ReadByte(_r.HL))); break; // RRC (HL)
				case 0x0F: _r.A = RotateRight(_r.A); break;	// RRC A

				case 0x18: _r.B = RotateRightThroughCarry(_r.B); break; // RR B
				case 0x19: _r.C = RotateRightThroughCarry(_r.C); break; // RR C
				case 0x1A: _r.D = RotateRightThroughCarry(_r.D); break; // RR D
				case 0x1B: _r.E = RotateRightThroughCarry(_r.E); break; // RR E
				case 0x1C: _r.H = RotateRightThroughCarry(_r.H); break; // RR H
				case 0x1D: _r.L = RotateRightThroughCarry(_r.L); break; // RR L
				case 0x1E: WriteByte(_r.HL, RotateRightThroughCarry(ReadByte(_r.HL))); break; // RR (HL)
				case 0x1F: _r.A = RotateRightThroughCarry(_r.A); break;	// RR A

				case 0x20: _r.B = ShiftLeftArithmetic(_r.B); break; // SLA B
				case 0x21: _r.C = ShiftLeftArithmetic(_r.C); break; // SLA C
				case 0x22: _r.D = ShiftLeftArithmetic(_r.D); break; // SLA D
				case 0x23: _r.E = ShiftLeftArithmetic(_r.E); break; // SLA E
				case 0x24: _r.H = ShiftLeftArithmetic(_r.H); break; // SLA H
				case 0x25: _r.L = ShiftLeftArithmetic(_r.L); break; // SLA L
				case 0x26: WriteByte(_r.HL, ShiftLeftArithmetic(ReadByte(_r.HL))); break; // SLA (HL)
				case 0x27: _r.A = ShiftLeftArithmetic(_r.A); break; // SLA A

				case 0x28: _r.B = ShiftRightArithmetic(_r.B); break; // SRA B
				case 0x29: _r.C = ShiftRightArithmetic(_r.C); break; // SRA C
				case 0x2A: _r.D = ShiftRightArithmetic(_r.D); break; // SRA D
				case 0x2B: _r.E = ShiftRightArithmetic(_r.E); break; // SRA E
				case 0x2C: _r.H = ShiftRightArithmetic(_r.H); break; // SRA H
				case 0x2D: _r.L = ShiftRightArithmetic(_r.L); break; // SRA L
				case 0x2E: WriteByte(_r.HL, ShiftRightArithmetic(ReadByte(_r.HL))); break; // SRA (HL)
				case 0x2F: _r.A = ShiftRightArithmetic(_r.A); break; // SRA A

				case 0x38: _r.B = ShiftRightLogical(_r.B); break; // SRL B
				case 0x39: _r.C = ShiftRightLogical(_r.C); break; // SRL C
				case 0x3A: _r.D = ShiftRightLogical(_r.D); break; // SRL D
				case 0x3B: _r.E = ShiftRightLogical(_r.E); break; // SRL E
				case 0x3C: _r.H = ShiftRightLogical(_r.H); break; // SRL H
				case 0x3D: _r.L = ShiftRightLogical(_r.L); break; // SRL L
				case 0x3E: WriteByte(_r.HL, ShiftRightLogical(ReadByte(_r.HL))); break; // SRL (HL)
				case 0x3F: _r.A = ShiftRightLogical(_r.A); break;	// SRL A

				case 0x30: _r.B = Swap(_r.B); break; // SWAP B
				case 0x31: _r.C = Swap(_r.C); break; // SWAP C
				case 0x32: _r.D = Swap(_r.D); break; // SWAP D
				case 0x33: _r.E = Swap(_r.E); break; // SWAP E
				case 0x34: _r.H = Swap(_r.H); break; // SWAP H
				case 0x35: _r.L = Swap(_r.L); break; // SWAP L
				case 0x36: WriteByte(_r.HL, Swap(ReadByte(_r.HL))); break; // SWAP (HL)
				case 0x37: _r.A = Swap(_r.A); break; // SWAP A

				#endregion

				#region bit operation instructions

				case 0x40: TestBit(_r.B, 0); break; // BIT 0,B
				case 0x41: TestBit(_r.C, 0); break; // BIT 0,C
				case 0x42: TestBit(_r.D, 0); break; // BIT 0,D
				case 0x43: TestBit(_r.E, 0); break; // BIT 0,E
				case 0x44: TestBit(_r.H, 0); break; // BIT 0,H
				case 0x45: TestBit(_r.L, 0); break; // BIT 0,L
				case 0x46: TestBit(ReadByte(_r.HL), 0); break; // BIT 0,(HL)
				case 0x47: TestBit(_r.A, 0); break; // BIT 0,A

				case 0x48: TestBit(_r.B, 1); break; // BIT 1,B
				case 0x49: TestBit(_r.C, 1); break; // BIT 1,C
				case 0x4A: TestBit(_r.D, 1); break; // BIT 1,D
				case 0x4B: TestBit(_r.E, 1); break; // BIT 1,E
				case 0x4C: TestBit(_r.H, 1); break; // BIT 1,H
				case 0x4D: TestBit(_r.L, 1); break; // BIT 1,L
				case 0x4E: TestBit(ReadByte(_r.HL), 1); break; // BIT 1,(HL)
				case 0x4F: TestBit(_r.A, 1); break; // BIT 1,A

				case 0x50: TestBit(_r.B, 2); break; // BIT 2,B
				case 0x51: TestBit(_r.C, 2); break; // BIT 2,C
				case 0x52: TestBit(_r.D, 2); break; // BIT 2,D
				case 0x53: TestBit(_r.E, 2); break; // BIT 2,E
				case 0x54: TestBit(_r.H, 2); break; // BIT 2,H
				case 0x55: TestBit(_r.L, 2); break; // BIT 2,L
				case 0x56: TestBit(ReadByte(_r.HL), 2); break; // BIT 2,(HL)
				case 0x57: TestBit(_r.A, 2); break; // BIT 2,A

				case 0x58: TestBit(_r.B, 3); break; // BIT 3,B
				case 0x59: TestBit(_r.C, 3); break; // BIT 3,C
				case 0x5A: TestBit(_r.D, 3); break; // BIT 3,D
				case 0x5B: TestBit(_r.E, 3); break; // BIT 3,E
				case 0x5C: TestBit(_r.H, 3); break; // BIT 3,H
				case 0x5D: TestBit(_r.L, 3); break; // BIT 3,L
				case 0x5E: TestBit(ReadByte(_r.HL), 3); break; // BIT 3,(HL)
				case 0x5F: TestBit(_r.A, 3); break; // BIT 3,A

				case 0x60: TestBit(_r.B, 4); break; // BIT 4,B
				case 0x61: TestBit(_r.C, 4); break; // BIT 4,C
				case 0x62: TestBit(_r.D, 4); break; // BIT 4,D
				case 0x63: TestBit(_r.E, 4); break; // BIT 4,E
				case 0x64: TestBit(_r.H, 4); break; // BIT 4,H
				case 0x65: TestBit(_r.L, 4); break; // BIT 4,L
				case 0x66: TestBit(ReadByte(_r.HL), 4); break; // BIT 4,(HL)
				case 0x67: TestBit(_r.A, 4); break; // BIT 4,A

				case 0x68: TestBit(_r.B, 5); break; // BIT 5,B
				case 0x69: TestBit(_r.C, 5); break; // BIT 5,C
				case 0x6A: TestBit(_r.D, 5); break; // BIT 5,D
				case 0x6B: TestBit(_r.E, 5); break; // BIT 5,E
				case 0x6C: TestBit(_r.H, 5); break; // BIT 5,H
				case 0x6D: TestBit(_r.L, 5); break; // BIT 5,L
				case 0x6E: TestBit(ReadByte(_r.HL), 5); break; // BIT 5,(HL)
				case 0x6F: TestBit(_r.A, 5); break; // BIT 5,A

				case 0x70: TestBit(_r.B, 6); break; // BIT 6,B
				case 0x71: TestBit(_r.C, 6); break; // BIT 6,C
				case 0x72: TestBit(_r.D, 6); break; // BIT 6,D
				case 0x73: TestBit(_r.E, 6); break; // BIT 6,E
				case 0x74: TestBit(_r.H, 6); break; // BIT 6,H
				case 0x75: TestBit(_r.L, 6); break; // BIT 6,L
				case 0x76: TestBit(ReadByte(_r.HL), 6); break; // BIT 6,(HL)
				case 0x77: TestBit(_r.A, 6); break; // BIT 6,A

				case 0x78: TestBit(_r.B, 7); break; // BIT 7,B
				case 0x79: TestBit(_r.C, 7); break; // BIT 7,C
				case 0x7A: TestBit(_r.D, 7); break; // BIT 7,D
				case 0x7B: TestBit(_r.E, 7); break; // BIT 7,E
				case 0x7C: TestBit(_r.H, 7); break; // BIT 7,H
				case 0x7D: TestBit(_r.L, 7); break; // BIT 7,L
				case 0x7E: TestBit(ReadByte(_r.HL), 7); break; // BIT 7,(HL)
				case 0x7F: TestBit(_r.A, 7); break; // BIT 7,A

				case 0xC0: _r.B = SetBit(_r.B, 0); break; // SET 0,B
				case 0xC1: _r.C = SetBit(_r.C, 0); break; // SET 0,C
				case 0xC2: _r.D = SetBit(_r.D, 0); break; // SET 0,D
				case 0xC3: _r.E = SetBit(_r.E, 0); break; // SET 0,E
				case 0xC4: _r.H = SetBit(_r.H, 0); break; // SET 0,H
				case 0xC5: _r.L = SetBit(_r.L, 0); break; // SET 0,L
				case 0xC6: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 0)); break; // SET 0,(HL)
				case 0xC7: _r.A = SetBit(_r.A, 0); break; // SET 0,A

				case 0xC8: _r.B = SetBit(_r.B, 1); break; // SET 1,B
				case 0xC9: _r.C = SetBit(_r.C, 1); break; // SET 1,C
				case 0xCA: _r.D = SetBit(_r.D, 1); break; // SET 1,D
				case 0xCB: _r.E = SetBit(_r.E, 1); break; // SET 1,E
				case 0xCC: _r.H = SetBit(_r.H, 1); break; // SET 1,H
				case 0xCD: _r.L = SetBit(_r.L, 1); break; // SET 1,L
				case 0xCE: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 1)); break; // SET 1,(HL)
				case 0xCF: _r.A = SetBit(_r.A, 1); break; // SET 1,A

				case 0xD0: _r.B = SetBit(_r.B, 2); break; // SET 2,B
				case 0xD1: _r.C = SetBit(_r.C, 2); break; // SET 2,C
				case 0xD2: _r.D = SetBit(_r.D, 2); break; // SET 2,D
				case 0xD3: _r.E = SetBit(_r.E, 2); break; // SET 2,E
				case 0xD4: _r.H = SetBit(_r.H, 2); break; // SET 2,H
				case 0xD5: _r.L = SetBit(_r.L, 2); break; // SET 2,L
				case 0xD6: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 2)); break; // SET 2,(HL)
				case 0xD7: _r.A = SetBit(_r.A, 2); break; // SET 2,A

				case 0xD8: _r.B = SetBit(_r.B, 3); break; // SET 3,B
				case 0xD9: _r.C = SetBit(_r.C, 3); break; // SET 3,C
				case 0xDA: _r.D = SetBit(_r.D, 3); break; // SET 3,D
				case 0xDB: _r.E = SetBit(_r.E, 3); break; // SET 3,E
				case 0xDC: _r.H = SetBit(_r.H, 3); break; // SET 3,H
				case 0xDD: _r.L = SetBit(_r.L, 3); break; // SET 3,L
				case 0xDE: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 3)); break; // SET 3,(HL)
				case 0xDF: _r.A = SetBit(_r.A, 3); break; // SET 3,A

				case 0xE0: _r.B = SetBit(_r.B, 4); break; // SET 4,B
				case 0xE1: _r.C = SetBit(_r.C, 4); break; // SET 4,C
				case 0xE2: _r.D = SetBit(_r.D, 4); break; // SET 4,D
				case 0xE3: _r.E = SetBit(_r.E, 4); break; // SET 4,E
				case 0xE4: _r.H = SetBit(_r.H, 4); break; // SET 4,H
				case 0xE5: _r.L = SetBit(_r.L, 4); break; // SET 4,L
				case 0xE6: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 4)); break; // SET 4,(HL)
				case 0xE7: _r.A = SetBit(_r.A, 4); break; // SET 4,A

				case 0xE8: _r.B = SetBit(_r.B, 5); break; // SET 5,B
				case 0xE9: _r.C = SetBit(_r.C, 5); break; // SET 5,C
				case 0xEA: _r.D = SetBit(_r.D, 5); break; // SET 5,D
				case 0xEB: _r.E = SetBit(_r.E, 5); break; // SET 5,E
				case 0xEC: _r.H = SetBit(_r.H, 5); break; // SET 5,H
				case 0xED: _r.L = SetBit(_r.L, 5); break; // SET 5,L
				case 0xEE: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 5)); break; // SET 5,(HL)
				case 0xEF: _r.A = SetBit(_r.A, 5); break; // SET 5,A

				case 0xF0: _r.B = SetBit(_r.B, 6); break; // SET 6,B
				case 0xF1: _r.C = SetBit(_r.C, 6); break; // SET 6,C
				case 0xF2: _r.D = SetBit(_r.D, 6); break; // SET 6,D
				case 0xF3: _r.E = SetBit(_r.E, 6); break; // SET 6,E
				case 0xF4: _r.H = SetBit(_r.H, 6); break; // SET 6,H
				case 0xF5: _r.L = SetBit(_r.L, 6); break; // SET 6,L
				case 0xF6: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 6)); break; // SET 6,(HL)
				case 0xF7: _r.A = SetBit(_r.A, 6); break; // SET 6,A

				case 0xF8: _r.B = SetBit(_r.B, 7); break; // SET 7,B
				case 0xF9: _r.C = SetBit(_r.C, 7); break; // SET 7,C
				case 0xFA: _r.D = SetBit(_r.D, 7); break; // SET 7,D
				case 0xFB: _r.E = SetBit(_r.E, 7); break; // SET 7,E
				case 0xFC: _r.H = SetBit(_r.H, 7); break; // SET 7,H
				case 0xFD: _r.L = SetBit(_r.L, 7); break; // SET 7,L
				case 0xFE: WriteByte(_r.HL, SetBit(ReadByte(_r.HL), 7)); break; // SET 7,(HL)
				case 0xFF: _r.A = SetBit(_r.A, 7); break; // SET 7,A

				case 0x80: _r.B = ResetBit(_r.B, 0); break; // RES 0,B
				case 0x81: _r.C = ResetBit(_r.C, 0); break; // RES 0,C
				case 0x82: _r.D = ResetBit(_r.D, 0); break; // RES 0,D
				case 0x83: _r.E = ResetBit(_r.E, 0); break; // RES 0,E
				case 0x84: _r.H = ResetBit(_r.H, 0); break; // RES 0,H
				case 0x85: _r.L = ResetBit(_r.L, 0); break; // RES 0,L
				case 0x86: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 0)); break; // RES 0,(HL)
				case 0x87: _r.A = ResetBit(_r.A, 0); break; // RES 0,A

				case 0x88: _r.B = ResetBit(_r.B, 1); break; // RES 1,B
				case 0x89: _r.C = ResetBit(_r.C, 1); break; // RES 1,C
				case 0x8A: _r.D = ResetBit(_r.D, 1); break; // RES 1,D
				case 0x8B: _r.E = ResetBit(_r.E, 1); break; // RES 1,E
				case 0x8C: _r.H = ResetBit(_r.H, 1); break; // RES 1,H
				case 0x8D: _r.L = ResetBit(_r.L, 1); break; // RES 1,L
				case 0x8E: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 1)); break; // RES 1,(HL)
				case 0x8F: _r.A = ResetBit(_r.A, 1); break; // RES 1,A

				case 0x90: _r.B = ResetBit(_r.B, 2); break; // RES 2,B
				case 0x91: _r.C = ResetBit(_r.C, 2); break; // RES 2,C
				case 0x92: _r.D = ResetBit(_r.D, 2); break; // RES 2,D
				case 0x93: _r.E = ResetBit(_r.E, 2); break; // RES 2,E
				case 0x94: _r.H = ResetBit(_r.H, 2); break; // RES 2,H
				case 0x95: _r.L = ResetBit(_r.L, 2); break; // RES 2,L
				case 0x96: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 2)); break; // RES 2,(HL)
				case 0x97: _r.A = ResetBit(_r.A, 2); break; // RES 2,A

				case 0x98: _r.B = ResetBit(_r.B, 3); break; // RES 3,B
				case 0x99: _r.C = ResetBit(_r.C, 3); break; // RES 3,C
				case 0x9A: _r.D = ResetBit(_r.D, 3); break; // RES 3,D
				case 0x9B: _r.E = ResetBit(_r.E, 3); break; // RES 3,E
				case 0x9C: _r.H = ResetBit(_r.H, 3); break; // RES 3,H
				case 0x9D: _r.L = ResetBit(_r.L, 3); break; // RES 3,L
				case 0x9E: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 3)); break; // RES 3,(HL)
				case 0x9F: _r.A = ResetBit(_r.A, 3); break; // RES 3,A

				case 0xA0: _r.B = ResetBit(_r.B, 4); break; // RES 4,B
				case 0xA1: _r.C = ResetBit(_r.C, 4); break; // RES 4,C
				case 0xA2: _r.D = ResetBit(_r.D, 4); break; // RES 4,D
				case 0xA3: _r.E = ResetBit(_r.E, 4); break; // RES 4,E
				case 0xA4: _r.H = ResetBit(_r.H, 4); break; // RES 4,H
				case 0xA5: _r.L = ResetBit(_r.L, 4); break; // RES 4,L
				case 0xA6: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 4)); break; // RES 4,(HL)
				case 0xA7: _r.A = ResetBit(_r.A, 4); break; // RES 4,A

				case 0xA8: _r.B = ResetBit(_r.B, 5); break; // RES 5,B
				case 0xA9: _r.C = ResetBit(_r.C, 5); break; // RES 5,C
				case 0xAA: _r.D = ResetBit(_r.D, 5); break; // RES 5,D
				case 0xAB: _r.E = ResetBit(_r.E, 5); break; // RES 5,E
				case 0xAC: _r.H = ResetBit(_r.H, 5); break; // RES 5,H
				case 0xAD: _r.L = ResetBit(_r.L, 5); break; // RES 5,L
				case 0xAE: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 5)); break; // RES 5,(HL)
				case 0xAF: _r.A = ResetBit(_r.A, 5); break; // RES 5,A

				case 0xB0: _r.B = ResetBit(_r.B, 6); break; // RES 6,B
				case 0xB1: _r.C = ResetBit(_r.C, 6); break; // RES 6,C
				case 0xB2: _r.D = ResetBit(_r.D, 6); break; // RES 6,D
				case 0xB3: _r.E = ResetBit(_r.E, 6); break; // RES 6,E
				case 0xB4: _r.H = ResetBit(_r.H, 6); break; // RES 6,H
				case 0xB5: _r.L = ResetBit(_r.L, 6); break; // RES 6,L
				case 0xB6: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 6)); break; // RES 6,(HL)
				case 0xB7: _r.A = ResetBit(_r.A, 6); break; // RES 6,A

				case 0xB8: _r.B = ResetBit(_r.B, 7); break; // RES 7,B
				case 0xB9: _r.C = ResetBit(_r.C, 7); break; // RES 7,C
				case 0xBA: _r.D = ResetBit(_r.D, 7); break; // RES 7,D
				case 0xBB: _r.E = ResetBit(_r.E, 7); break; // RES 7,E
				case 0xBC: _r.H = ResetBit(_r.H, 7); break; // RES 7,H
				case 0xBD: _r.L = ResetBit(_r.L, 7); break; // RES 7,L
				case 0xBE: WriteByte(_r.HL, ResetBit(ReadByte(_r.HL), 7)); break; // RES 7,(HL)
				case 0xBF: _r.A = ResetBit(_r.A, 7); break; // RES 7,A

				#endregion
					
				default:
					throw new NotImplementedException(string.Format("Invalid extended opcode 0x{0:X4} at 0x{1:X4}", 0xCB00 | opcode, _r.PC - 2));
			}
		}
		
		#region jump instruction handlers

		private void JumpImmediate(bool condition = true)
		{
			ushort address = ReadWord(_r.PC);

			if (condition)
			{
				_r.PC = address;
				_gb.Clock.AddMachineCycles(1);
			}
			else
			{
				// jump not taken, skip address
				_r.PC += 2;
			}
		}

		private void JumpRelative(bool condition = true)
		{
			sbyte offset = (sbyte)ReadByte(_r.PC++);

			if (condition)
			{
				_r.PC += (ushort)offset;
				_gb.Clock.AddMachineCycles(1);
			}
		}

		#endregion

		#region call instruction handlers

		private void CallImmediate(bool condition = true)
		{
			ushort address = ReadWord(_r.PC);
			_r.PC += 2;

			if (condition)
			{
				_gb.Clock.AddMachineCycles(1);

				PushWord(_r.PC);
				_r.PC = address;				
			}
		}

		private void Return()
		{
			_r.PC = PopWord();
			_gb.Clock.AddMachineCycles(1);
		}

		private void Return(bool condition)
		{
			if (condition)
			{
				_r.PC = PopWord();
				_gb.Clock.AddMachineCycles(1);
			}

			_gb.Clock.AddMachineCycles(1);
		}

		private void Reset(byte resetAddress)
		{
			_gb.Clock.AddMachineCycles(1);

			PushWord(_r.PC);

			_r.PC = resetAddress;
		}

		#endregion

		#region 8-bit arithmetic and logical operation instruction handlers

		private byte Add8Bit(byte b1, byte b2)
		{
			int result = b1 + b2;

			// clear flags
			_r.F = StatusFlags.Clear;

			// carry
			if (result > 0xFF)
				_r.F |= StatusFlags.C;

			// half carry
			if ((b1 & 0x0F) + (b2 & 0x0F) > 0x0F)
				_r.F |= StatusFlags.H;

			// zero 
			if ((result & 0xFF) == 0)
				_r.F |= StatusFlags.Z;

			return (byte)result;
		}

		private byte AddWithCarry8Bit(byte b1, byte b2)
		{
			int carry = _r.F.FlagSet(StatusFlags.C) ? 1 : 0;

			int result = b1 + b2 + carry;

			// clear flags
			_r.F = StatusFlags.Clear;

			// carry
			if (result > 0xFF)
				_r.F |= StatusFlags.C;

			// half carry
			if ((b1 & 0x0F) + (b2 & 0x0F) + carry > 0x0F)
				_r.F |= StatusFlags.H;

			// zero 
			if ((result & 0xFF) == 0)
				_r.F |= StatusFlags.Z;

			return (byte)result;
		}

		private byte Sub8Bit(byte b1, byte b2)
		{
			int result = b1 - b2;

			// set subtract
			_r.F = StatusFlags.N;

			// carry
			if (result < 0x00)
				_r.F |= StatusFlags.C;

			// half carry
			if ((b1 & 0x0F) - (b2 & 0x0F) < 0x00)
				_r.F |= StatusFlags.H;

			// zero 
			if ((result & 0xFF) == 0)
				_r.F |= StatusFlags.Z;

			return (byte)result;
		}

		private byte SubWithCarry8Bit(byte b1, byte b2)
		{
			int carry = _r.F.FlagSet(StatusFlags.C) ? 1 : 0;

			int result = b1 - b2 - carry;

			// set subtract
			_r.F = StatusFlags.N;

			// carry
			if (result < 0x00)
				_r.F |= StatusFlags.C;

			// half carry
			if ((b1 & 0x0F) - (b2 & 0x0F) - carry < 0x00)
				_r.F |= StatusFlags.H;

			// zero 
			if ((result & 0xFF) == 0)
				_r.F |= StatusFlags.Z;

			return (byte)result;
		}

		private byte And8Bit(byte b1, byte b2)
		{
			byte result = (byte)(b1 & b2);

			// set half carry
			_r.F = StatusFlags.H;

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte Or8Bit(byte b1, byte b2)
		{
			byte result = (byte)(b1 | b2);

			// clear flags
			_r.F = StatusFlags.Clear;

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte Xor8Bit(byte b1, byte b2)
		{
			byte result = (byte)(b1 ^ b2);

			// clear flags
			_r.F = StatusFlags.Clear;

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private void Compare8Bit(byte b1, byte b2)
		{
			int result = b1 - b2;

			// set subtract
			_r.F = StatusFlags.N;

			// carry
			if (b1 < b2)
				_r.F |= StatusFlags.C;

			// half carry
			if ((b1 & 0x0F) < (b2 & 0x0F))
				_r.F |= StatusFlags.H;

			// zero 
			if (b1 == b2)
				_r.F |= StatusFlags.Z;
		}

		private byte Inc8Bit(byte b)
		{
			// preserve carry
			_r.F &= StatusFlags.C;

			// half carry, if 0x0F before increment will carry
			if ((b & 0x0F) == 0x0F)
				_r.F |= StatusFlags.H;

			// zero, if 0xFF before increment will inc to zero
			if (b == 0xFF)
				_r.F |= StatusFlags.Z;

			return (byte)(b + 1);
		}

		private byte Dec8Bit(byte b)
		{
			// preserve carry
			_r.F &= StatusFlags.C;

			// set subtract
			_r.F |= StatusFlags.N;

			// half carry, if 0x00 before decrement will borrow
			if ((b & 0x0F) == 0x00)
				_r.F |= StatusFlags.H;

			// zero, if 0x01 before decrement will dec to zero
			if (b == 0x01)
				_r.F |= StatusFlags.Z;

			return (byte)(b - 1);
		}

		#endregion

		#region 16-bit arithmetic operation instruction handlers

		private ushort Add16Bit(ushort u1, ushort u2)
		{
			int result = u1 + u2;

			// preserve zero
			_r.F &= StatusFlags.Z;

			// carry
			if (result > 0xFFFF)
				_r.F |= StatusFlags.C;

			// half carry
			if ((u1 & 0x0FFF) + (u2 & 0x0FFF) > 0x0FFF)
				_r.F |= StatusFlags.H;

			return (ushort)result;
		}

		#endregion

		#region rotate shift instruction handlers

		private byte RotateLeft(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// grab bit 7
			bool bit7 = ((b & 0x80) == 0x80);
			
			// set carry flag if old bit 7 was set
			if (bit7)
			{
				_r.F |= StatusFlags.C;
			}

			// shift left and place old bit 7 in LSB
			byte result = (byte)((b << 1) | (bit7 ? 1 : 0));

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte RotateLeftThroughCarry(byte b)
		{
			bool oldCarry = _r.F.FlagSet(StatusFlags.C);

			// clear flags
			_r.F = StatusFlags.Clear;

			// set carry if top bit is set
			if ((b & 0x80) == 0x80)
				_r.F |= StatusFlags.C;

			// shift left and place old carry in LSB
			byte result = (byte)((b << 1) | (oldCarry ? 1 : 0));

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte RotateRight(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// grab bit 0
			bool bit0 = ((b & 0x01) == 0x01);

			// set carry flag if old bit 0 was set
			if (bit0)
			{
				_r.F |= StatusFlags.C;
			}

			// shift right and place old bit 0 in MSB
			byte result = (byte)((b >> 1) | (bit0 ? 0x80 : 0));

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte RotateRightThroughCarry(byte b)
		{
			bool oldCarry = _r.F.FlagSet(StatusFlags.C);

			// clear flags
			_r.F = StatusFlags.Clear;

			// set carry if bottom bit is set
			if ((b & 0x01) == 0x01)
				_r.F |= StatusFlags.C;

			// shift left and place old carry in MSB
			byte result = (byte)((b >> 1) | (oldCarry ? 0x80 : 0));

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte ShiftLeftArithmetic(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// set carry if top bit is set
			if ((b & 0x80) == 0x80)
				_r.F |= StatusFlags.C;

			// shift left
			byte result = (byte)(b << 1);

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte ShiftRightArithmetic(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// grab bit 0
			bool bit0 = ((b & 0x01) == 0x01);

			// grab bit 7
			byte bit7 = (byte)(b & 0x80);

			// set carry flag if old bit 0 was set
			if (bit0)
			{
				_r.F |= StatusFlags.C;
			}

			// shift right, set bit 7 to previous value (sign extend)
			byte result = (byte)((b >> 1) | bit7);

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte ShiftRightLogical(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// grab bit 0
			bool bit0 = ((b & 0x01) == 0x01);

			// set carry flag if old bit 0 was set
			if (bit0)
			{
				_r.F |= StatusFlags.C;
			}

			// shift right
			byte result = (byte)(b >> 1);

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return result;
		}

		private byte Swap(byte b)
		{
			// clear flags
			_r.F = StatusFlags.Clear;

			// swap high and low nibble

			var result = ((b & 0xF0) >> 4) | ((b & 0x0F) << 4);

			// zero
			if (result == 0)
				_r.F |= StatusFlags.Z;

			return (byte)result;
		}

		#endregion

		#region bit operation instruction handlers

		// test if bit bit is set in byte reg
		private void TestBit(byte reg, int bit)
		{
			// preserve carry
			_r.F &= StatusFlags.C;

			// set half carry
			_r.F |= StatusFlags.H;

			// set zero flag if bit N of reg is not set
			if ((reg & (1 << bit)) == 0)
			{
				_r.F |= StatusFlags.Z;
			}
		}

		private byte SetBit(byte reg, int bit)
		{
			return (byte)(reg | (1 << bit));
		}

		private byte ResetBit(byte reg, int bit)
		{
			return (byte)(reg & ~(1 << bit));
		}

		#endregion

		#region arithmetic and cpu control instruction handlers

		private void DecimalAdjustAccumulator()
		{
			byte correctionFactor = 0;
			bool subtraction = _r.F.FlagSet(StatusFlags.N);

			if ((_r.A > 0x99 && !subtraction) || _r.F.FlagSet(StatusFlags.C))
			{
				correctionFactor |= 0x60;
				_r.F |= StatusFlags.C;
			}
			else
				_r.F &= ~StatusFlags.C;

			if (((_r.A & 0x0F) > 0x09 && !subtraction) || _r.F.FlagSet(StatusFlags.H))
				correctionFactor |= 0x06;

			if (!subtraction)
				_r.A += correctionFactor;
			else
				_r.A -= correctionFactor;

			_r.F &= ~(StatusFlags.Z | StatusFlags.H);

			if (_r.A == 0)
				_r.F |= StatusFlags.Z;
		}

		#endregion
	}
}
