﻿using System;
using System.IO;

namespace elbgb_core.Memory
{
    enum BootRomType
    {
        Dmg,
        Dmg0,
        Mgb,
        Sgb,
        Sgb2
    }

    class BootRom : IMemoryMappedComponent
    {
        #region GameBoy - DMG
        private static readonly byte[] DmgBootRom =
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
        #endregion

        #region GameBoy - DMG0
        // https://board.byuu.org/viewtopic.php?f=8&t=886&start=80#p26654
        private static readonly byte[] Dmg0BootRom =
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
        #endregion

        #region GameBoy Pocket
        private static readonly byte[] MgbBootRom =
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
            0xF5,0x06,0x19,0x78,0x86,0x23,0x05,0x20,0xFB,0x86,0x20,0xFE,0x3E,0xFF,0xE0,0x50
        };
        #endregion

        #region Super GameBoy
        private static readonly byte[] SgbBootRom =
        {
            0x31,0xFE,0xFF,0x3E,0x30,0xE0,0x00,0xAF,0x21,0xFF,0x9F,0x32,0xCB,0x7C,0x20,0xFB,
            0x21,0x26,0xFF,0x0E,0x11,0x3E,0x80,0x32,0xE2,0x0C,0x3E,0xF3,0xE2,0x32,0x3E,0x77,
            0x77,0x3E,0xFC,0xE0,0x47,0x21,0x5F,0xC0,0x0E,0x08,0xAF,0x32,0x0D,0x20,0xFC,0x11,
            0x4F,0x01,0x3E,0xFB,0x0E,0x06,0xF5,0x06,0x00,0x1A,0x1B,0x32,0x80,0x47,0x0D,0x20,
            0xF8,0x32,0xF1,0x32,0x0E,0x0E,0xD6,0x02,0xFE,0xEF,0x20,0xEA,0x11,0x04,0x01,0x21,
            0x10,0x80,0x1A,0xCD,0xD3,0x00,0xCD,0xD4,0x00,0x13,0x7B,0xFE,0x34,0x20,0xF3,0x11,
            0xE6,0x00,0x06,0x08,0x1A,0x13,0x22,0x23,0x05,0x20,0xF9,0x3E,0x19,0xEA,0x10,0x99,
            0x21,0x2F,0x99,0x0E,0x0C,0x3D,0x28,0x08,0x32,0x0D,0x20,0xF9,0x2E,0x0F,0x18,0xF3,
            0x3E,0x91,0xE0,0x40,0x21,0x00,0xC0,0x0E,0x00,0x3E,0x00,0xE2,0x3E,0x30,0xE2,0x06,
            0x10,0x1E,0x08,0x2A,0x57,0xCB,0x42,0x3E,0x10,0x20,0x02,0x3E,0x20,0xE2,0x3E,0x30,
            0xE2,0xCB,0x1A,0x1D,0x20,0xEF,0x05,0x20,0xE8,0x3E,0x20,0xE2,0x3E,0x30,0xE2,0xCD,
            0xC2,0x00,0x7D,0xFE,0x60,0x20,0xD2,0x0E,0x13,0x3E,0xC1,0xE2,0x0C,0x3E,0x07,0xE2,
            0x18,0x3A,0x16,0x04,0xF0,0x44,0xFE,0x90,0x20,0xFA,0x1E,0x00,0x1D,0x20,0xFD,0x15,
            0x20,0xF2,0xC9,0x4F,0x06,0x04,0xC5,0xCB,0x11,0x17,0xC1,0xCB,0x11,0x17,0x05,0x20,
            0xF5,0x22,0x23,0x22,0x23,0xC9,0x3C,0x42,0xB9,0xA5,0xB9,0xA5,0x42,0x3C,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3E,0x01,0xE0,0x50
        };
        #endregion

        #region Super GameBoy 2
        private static readonly byte[] Sgb2BootRom =
        {
            0x31,0xFE,0xFF,0x3E,0x30,0xE0,0x00,0xAF,0x21,0xFF,0x9F,0x32,0xCB,0x7C,0x20,0xFB,
            0x21,0x26,0xFF,0x0E,0x11,0x3E,0x80,0x32,0xE2,0x0C,0x3E,0xF3,0xE2,0x32,0x3E,0x77,
            0x77,0x3E,0xFC,0xE0,0x47,0x21,0x5F,0xC0,0x0E,0x08,0xAF,0x32,0x0D,0x20,0xFC,0x11,
            0x4F,0x01,0x3E,0xFB,0x0E,0x06,0xF5,0x06,0x00,0x1A,0x1B,0x32,0x80,0x47,0x0D,0x20,
            0xF8,0x32,0xF1,0x32,0x0E,0x0E,0xD6,0x02,0xFE,0xEF,0x20,0xEA,0x11,0x04,0x01,0x21,
            0x10,0x80,0x1A,0xCD,0xD3,0x00,0xCD,0xD4,0x00,0x13,0x7B,0xFE,0x34,0x20,0xF3,0x11,
            0xE6,0x00,0x06,0x08,0x1A,0x13,0x22,0x23,0x05,0x20,0xF9,0x3E,0x19,0xEA,0x10,0x99,
            0x21,0x2F,0x99,0x0E,0x0C,0x3D,0x28,0x08,0x32,0x0D,0x20,0xF9,0x2E,0x0F,0x18,0xF3,
            0x3E,0x91,0xE0,0x40,0x21,0x00,0xC0,0x0E,0x00,0x3E,0x00,0xE2,0x3E,0x30,0xE2,0x06,
            0x10,0x1E,0x08,0x2A,0x57,0xCB,0x42,0x3E,0x10,0x20,0x02,0x3E,0x20,0xE2,0x3E,0x30,
            0xE2,0xCB,0x1A,0x1D,0x20,0xEF,0x05,0x20,0xE8,0x3E,0x20,0xE2,0x3E,0x30,0xE2,0xCD,
            0xC2,0x00,0x7D,0xFE,0x60,0x20,0xD2,0x0E,0x13,0x3E,0xC1,0xE2,0x0C,0x3E,0x07,0xE2,
            0x18,0x3A,0x16,0x04,0xF0,0x44,0xFE,0x90,0x20,0xFA,0x1E,0x00,0x1D,0x20,0xFD,0x15,
            0x20,0xF2,0xC9,0x4F,0x06,0x04,0xC5,0xCB,0x11,0x17,0xC1,0xCB,0x11,0x17,0x05,0x20,
            0xF5,0x22,0x23,0x22,0x23,0xC9,0x3C,0x42,0xB9,0xA5,0xB9,0xA5,0x42,0x3C,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3E,0xFF,0xE0,0x50
        };
        #endregion

        private Interconnect _interconnect;

        private byte _lockout;
        private byte[] _bootRom;

        private BootRom(Interconnect interconnect)
        {
            _interconnect = interconnect;

            interconnect.AddAddressHandler(0x0000, 0x00FF, this);
            interconnect.AddAddressHandler(0xFF50, this);

            _lockout = 0xFE;
        }

        public BootRom(Interconnect interconnect, BootRomType bootRomType)
            : this(interconnect)
        {
            switch (bootRomType)
            {
                case BootRomType.Dmg:
                    _bootRom = DmgBootRom;
                    break;

                case BootRomType.Dmg0:
                    _bootRom = Dmg0BootRom;
                    break;

                case BootRomType.Mgb:
                    _bootRom = MgbBootRom;
                    break;

                case BootRomType.Sgb:
                    _bootRom = SgbBootRom;
                    break;

                case BootRomType.Sgb2:
                    _bootRom = Sgb2BootRom;
                    break;

                default:
                    throw new ArgumentException("Invalid boot rom type", nameof(bootRomType));
            }
        }

        public BootRom(Interconnect interconnect, byte[] bootRom)
            : this(interconnect)
        {
            _bootRom = bootRom;
        }

        public byte ReadByte(ushort address)
        {
            if (address == 0xFF50)
            {
                return _lockout;
            }
            else
            {
                return _bootRom[address];
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            if (address == 0xFF50)
            {
                _lockout = (byte)(value | 0xFE);

                // a write to FF50 removes the mapping to the boot rom, copy the mapping
                // from 0x0100 as this will either be the cartridge if present, or unmapped
                _interconnect.CopyAddressHandler(0x0000, 0x00FF, 0x0100);
            }
        }
    }
}