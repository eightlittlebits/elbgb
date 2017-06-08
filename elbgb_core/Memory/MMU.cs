using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_core.Memory
{
    public class MMU
    {
        public static class Registers
        {
            public const ushort BOOTROMLOCK = 0xFF50;
        }

        private GameBoy _gb;

        private bool _bootRomLocked;

#if true
        // widely distributed boot rom
        private byte[] _bootRom =
        {
            0x31,0xFE,0xFF,0xAF,0x21,0xFF,0x9F,0x32,0xCB,0x7C,0x20,0xFB,0x21,0x26,0xFF,0x0E,
            0x11,0x3E,0x80,0x32,0xE2,0x0C,0x3E,0xF3,0xE2,0x32,0x3E,0x77,0x77,0x3E,0xFC,0xE0,
            0x47,0x11,0x04,0x01,0x21,0x10,0x80,0x1A,0xCD,0x95,0x00,0xCD,0x96,0x00,0x13,0x7B,
            0xFE,0x34,0x20,0xF3,0x11,0xD8,0x00,0x06,0x08,0x1A,0x13,0x22,0x23,0x05,0x20,0xF9,
            0x3E,0x19,0xEA,0x10,0x99,0x21,0x2F,0x99,0x0E,0x0C,0x3D,0x28,0x08,0x32,0x0D,0x20,
            0xF9,0x2E,0x0F,0x18,0xF3,0x67,0x3E,0x64,0x57,0xE0,0x42,0x3E,0x91,0xE0,0x40,0x04,
            0x1E,0x02,0x0E,0x0C,0xF0,0x44,0xFE,0x90,0x20,0xFA,0x0D,0x20,0xF7,0x1D,0x20,0xF2,
            0x0E,0x13,0x24,0x7C,0x1E,0x83,0xFE,0x62,0x28,0x06,0x1E,0xC1,0xFE,0x64,0x20,0x06,
            0x7B,0xE2,0x0C,0x3E,0x87,0xE2,0xF0,0x42,0x90,0xE0,0x42,0x15,0x20,0xD2,0x05,0x20,
            0x4F,0x16,0x20,0x18,0xCB,0x4F,0x06,0x04,0xC5,0xCB,0x11,0x17,0xC1,0xCB,0x11,0x17,
            0x05,0x20,0xF5,0x22,0x23,0x22,0x23,0xC9,0xCE,0xED,0x66,0x66,0xCC,0x0D,0x00,0x0B,
            0x03,0x73,0x00,0x83,0x00,0x0C,0x00,0x0D,0x00,0x08,0x11,0x1F,0x88,0x89,0x00,0x0E,
            0xDC,0xCC,0x6E,0xE6,0xDD,0xDD,0xD9,0x99,0xBB,0xBB,0x67,0x63,0x6E,0x0E,0xEC,0xCC,
            0xDD,0xDC,0x99,0x9F,0xBB,0xB9,0x33,0x3E,0x3C,0x42,0xB9,0xA5,0xB9,0xA5,0x42,0x3C,
            0x21,0x04,0x01,0x11,0xA8,0x00,0x1A,0x13,0xBE,0x20,0xFE,0x23,0x7D,0xFE,0x34,0x20,
            0xF5,0x06,0x19,0x78,0x86,0x23,0x05,0x20,0xFB,0x86,0x20,0xFE,0x3E,0x01,0xE0,0x50
        };
#else
        // original boot rom
        // http://board.byuu.org/phpbb3/viewtopic.php?f=8&t=886&start=80#p26654
        private byte[] _bootRom = 
        {
            0x31,0xFE,0xFF,0xAF,0x21,0xFF,0x9F,0x32,0xCB,0x7C,0x20,0xFB,0x21,0x26,0xFF,0x0E,
            0x11,0x3E,0x80,0x32,0xE2,0x0C,0x3E,0xF3,0xE2,0x32,0x3E,0x77,0x77,0x3E,0xFC,0xE0,
            0x47,0x21,0x04,0x01,0xE5,0x11,0xCB,0x00,0x1A,0x13,0xBE,0x20,0x6B,0x23,0x7D,0xFE,
            0x34,0x20,0xF5,0x06,0x19,0x78,0x86,0x23,0x05,0x20,0xFB,0x86,0x20,0x5A,0xD1,0x21,
            0x10,0x80,0x1A,0xCD,0xA9,0x00,0xCD,0xAA,0x00,0x13,0x7B,0xFE,0x34,0x20,0xF3,0x3E,
            0x18,0x21,0x2F,0x99,0x0E,0x0C,0x32,0x3D,0x28,0x09,0x0D,0x20,0xF9,0x11,0xEC,0xFF,
            0x19,0x18,0xF1,0x67,0x3E,0x64,0x57,0xE0,0x42,0x3E,0x91,0xE0,0x40,0x04,0x1E,0x02,
            0xCD,0xBC,0x00,0x0E,0x13,0x24,0x7C,0x1E,0x83,0xFE,0x62,0x28,0x06,0x1E,0xC1,0xFE,
            0x64,0x20,0x06,0x7B,0xE2,0x0C,0x3E,0x87,0xE2,0xF0,0x42,0x90,0xE0,0x42,0x15,0x20,
            0xDD,0x05,0x20,0x69,0x16,0x20,0x18,0xD6,0x3E,0x91,0xE0,0x40,0x1E,0x14,0xCD,0xBC,
            0x00,0xF0,0x47,0xEE,0xFF,0xE0,0x47,0x18,0xF3,0x4F,0x06,0x04,0xC5,0xCB,0x11,0x17,
            0xC1,0xCB,0x11,0x17,0x05,0x20,0xF5,0x22,0x23,0x22,0x23,0xC9,0x0E,0x0C,0xF0,0x44,
            0xFE,0x90,0x20,0xFA,0x0D,0x20,0xF7,0x1D,0x20,0xF2,0xC9,0xCE,0xED,0x66,0x66,0xCC,
            0x0D,0x00,0x0B,0x03,0x73,0x00,0x83,0x00,0x0C,0x00,0x0D,0x00,0x08,0x11,0x1F,0x88,
            0x89,0x00,0x0E,0xDC,0xCC,0x6E,0xE6,0xDD,0xDD,0xD9,0x99,0xBB,0xBB,0x67,0x63,0x6E,
            0x0E,0xEC,0xCC,0xDD,0xDC,0x99,0x9F,0xBB,0xB9,0x33,0x3E,0xFF,0xFF,0x3C,0xE0,0x50
        };
#endif

        private byte[] _wram;
        private byte[] _hram;

        public MMU(GameBoy gameBoy)
        {
            _gb = gameBoy;

            _bootRomLocked = false;

            _wram = new byte[0x2000];
            _hram = new byte[0x7F];
        }

        public byte ReadByte(ushort address)
        {
            switch (address & 0xF000)
            {
                // 0x0000 - 0x0100 - boot rom
                // 0x0000 - 0x7FFF - rom 
                case 0x0000:
                    if (!_bootRomLocked && address < 0x100)
                    {
                        return _bootRom[address];
                    }

                    return _gb.Cartridge.ReadByte(address);

                // 0x0000 - 0x7FFF - rom
                case 0x1000: case 0x2000: case 0x3000: case 0x4000:
                case 0x5000: case 0x6000: case 0x7000:
                    return _gb.Cartridge.ReadByte(address);

                // 0x8000 - 0x9FFF - vram
                case 0x8000:
                case 0x9000:
                    return _gb.LCD.ReadByte(address);

                // 0xA000 - 0xBFFF - external expansion RAM
                case 0xA000:
                case 0xB000:
                    return _gb.Cartridge.ReadByte(address);

                // 0xC000 - 0xDFFF - working ram
                case 0xC000:
                case 0xD000:
                    return _wram[address & 0x1FFF];

                // 0xE000 - 0xFDFF mirrors working ram
                case 0xE000:
                    return _wram[address & 0x1FFF];

                case 0xF000:
                    // 0xE000 - 0xFDFF mirrors working ram
                    if (address <= 0xFDFF)
                    {
                        return _wram[address & 0x1FFF];
                    }
                    // 0xFE00 - 0xFE9F - OAM memory
                    else if (address >= 0xFE00 && address <= 0xFE9F)
                    {
                        return _gb.LCD.ReadByte(address);
                    }
                    // 0xFEA0 - 0xFEFF - restricted area - return 0
                    else if (address >= 0xFEA0 && address <= 0xFEFF)
                    {
                        return 0x00;
                    }
                    // 0xFF00 - input
                    else if (address == 0xFF00)
                    {
                        return _gb.Input.ReadByte(address);
                    }
                    // 0xFF01 - 0xFF02 - serial I/O
                    else if (address >= 0xFF01 && address <= 0xFF02)
                    {
                        return _gb.SerialIO.ReadByte(address);
                    }
                    // 0xFF04 - 0xFF07 - timer registers
                    else if (address >= 0xFF04 && address <= 0xFF07)
                    {
                        return _gb.Timer.ReadByte(address);
                    }
                    // 0xFF0F - interrupt flag
                    else if (address == 0xFF0F)
                    {
                        return _gb.InterruptController.ReadByte(address);
                    }
                    // 0xFF10 - 0xFF26 - NR xx sound registers
                    // 0xFF30 - 0xFF3F - waveform ram
                    else if (address >= 0xFF10 && address <= 0xFF3F)
                    {
                        return _gb.PSG.ReadByte(address);
                    }
                    // 0xFF40 - 0xFF4B - lcd registers
                    else if (address >= 0xFF40 && address <= 0xFF4B)
                    {
                        return _gb.LCD.ReadByte(address);
                    }
                    // 0xFF80 - 0xFFFE - hi ram
                    else if (address >= 0xFF80 && address <= 0xFFFE)
                    {
                        return _hram[address & 0x7F];
                    }
                    else if (address == 0xFFFF)
                    {
                        return _gb.InterruptController.ReadByte(address);
                    }
                    break;
            }

            // return 0xFF for any unhandled addresses
            Trace.WriteLine($"Unhandled address read: 0x{address:X}", "MMU");
            return 0xFF;
        }

        public void WriteByte(ushort address, byte value)
        {
            switch (address & 0xF000)
            {
                // 0x0000 - 0x7FFF - rom
                case 0x0000: case 0x1000: case 0x2000: case 0x3000:
                case 0x4000: case 0x5000: case 0x6000: case 0x7000:
                    _gb.Cartridge.WriteByte(address, value);
                    return;

                // 0x8000 - 0x9FFF - vram
                case 0x8000:
                case 0x9000:
                    _gb.LCD.WriteByte(address, value);
                    return;

                // 0xA000 - 0xBFFF - external expansion RAM
                case 0xA000:
                case 0xB000:
                    _gb.Cartridge.WriteByte(address, value);
                    return;

                // 0xC000 - 0xDFFF - working ram
                case 0xC000:
                case 0xD000:
                    _wram[address & 0x1FFF] = value;
                    return;

                // 0xE000 - 0xFDFF mirrors working ram
                case 0xE000:
                    _wram[address & 0x1FFF] = value;
                    return;

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // 0xE000 - 0xFDFF mirrors working ram
                        case 0x000: case 0x100: case 0x200: case 0x300:
                        case 0x400: case 0x500: case 0x600: case 0x700:
                        case 0x800: case 0x900: case 0xA00: case 0xB00:
                        case 0xC00: case 0xD00:
                            _wram[address & 0x1FFF] = value;
                            return;

                        // 0xFE00 - 0xFE9F - OAM memory
                        // 0xFEA0 - 0xFEFF - restricted area - ignore any writes  
                        case 0xE00:
                            if (address <= 0xFE9F)
                            {
                                _gb.LCD.WriteByte(address, value);
                            }
                            else
                            {
                                // Ignore writes to 0xFEA0 - 0xFEFF
                            }
                            return;

                        case 0xF00:
                            switch (address & 0x00F0)
                            {
                                case 0x00:
                                    switch (address & 0x000F)
                                    {
                                        // 0xFF00 - input
                                        case 0x0: // P1
                                            _gb.Input.WriteByte(address, value);
                                            return;

                                        // 0xFF01 - 0xFF02 - serial I/O
                                        case 0x1: // SB
                                        case 0x2: // SC
                                            _gb.SerialIO.WriteByte(address, value);
                                            return;
                                        
                                        // 0xFF04 - 0xFF07 - timer registers
                                        case 0x4: // DIV
                                        case 0x5: // TIMA
                                        case 0x6: // TMA
                                        case 0x7: // TAC
                                            _gb.Timer.WriteByte(address, value);
                                            return;
                                        
                                        // 0xFF0F - interrupt flag
                                        case 0xF: // IF
                                            _gb.InterruptController.WriteByte(address, value);
                                            return;
                                    }
                                    break;

                                // 0xFF10 - 0xFF26 - NR xx sound registers
                                case 0x10:
                                case 0x20:
                                // 0xFF30 - 0xFF3F - waveform ram
                                case 0x30:
                                    _gb.PSG.WriteByte(address, value); return;

                                // 0xFF40 - 0xFF4B - lcd registers
                                case 0x40:
                                    _gb.LCD.WriteByte(address, value); return;

                                case 0x50: 
                                    switch (address & 0x000F)
                                    {
                                        // 0xFF50 - boot rom lock
                                        case 0x0: 
                                            _bootRomLocked = true;
                                            return;
                                    }
                                    break;
                                
                                // 0xFF80 - 0xFFFE - hi ram
                                case 0x80: case 0x90: case 0xA0: case 0xB0:
                                case 0xC0: case 0xD0: case 0xE0: 
                                    _hram[address & 0x7F] = value;
                                    return;

                                case 0xF0:
                                    switch (address & 0x000F)
                                    {
                                        default:
                                            _hram[address & 0x7F] = value;
                                            return;

                                        // 0xFFFF - interrupt enable
                                        case 0xF: // IE
                                            _gb.InterruptController.WriteByte(address, value);
                                            return;
                                    }
                            }
                            break;
                    }
                    break;
            }

            Trace.WriteLine($"Unhandled address write: 0x{address:X}", "MMU");
        }
    }
}
