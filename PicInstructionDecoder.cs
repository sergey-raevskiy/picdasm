﻿using System;

namespace picdasm
{
    enum PicInstructionKind
    {
        Unknown,
        Misc,
        Literal,
        Alu,
        Alu2,
        BraRCall,
    }

    enum PicMiscInstruction : byte
    {
        NOP = 0x00,
        SLEEP = 0x03,
        CLRWDT = 0x04,
        PUSH = 0x05,
        POP = 0x06,
        DAW = 0x07,
        TBLRD = 0x08,
        TBLRDPostInc = 0x09,
        TBLRDPostDec = 0x0A,
        TBLRDPreInc = 0x0B,
        TBLWR = 0x0C,
        TBLWRPostInc = 0x0D,
        TBLWRPostDec = 0x0E,
        TBLWRPreInc = 0x0F,
        RETFIE = 0x10,
        RETFIEFast = 0x11,
        RETURN = 0x12,
        RETURNFast = 0x13,
        CALLW = 0x14,
        RESET = 0xFF,
    }

    enum PicLiteralInstruction : byte
    {
        // 0 0 0 0  1 opcode:3 literal:8

        Prefix = 0x08,
        PrefixMask = 0xF8,
        OpCodeMask = 0x07,

        SUBLW = 0x0,
        IORLW = 0x1,
        XORLW = 0x2,
        ANDLW = 0x3,
        RETLW = 0x4,
        MULLW = 0x5,
        MOVLW = 0x6,
        ADDLW = 0x7,
    }

    enum PicAluInstruction : byte
    {
        // 0 opcode:5 d a f:8

        Prefix = 0x00,
        PrefixMask = 0x80,
        OpCodeMask = 0x7C,

        MULWF = 0x00,
        DECF = 0x04,
        IORWF = 0x10,
        ANDWF = 0x14,
        XORWF = 0x18,
        COMF = 0x1C,
        ADDWFC = 0x20,
        ADDWF = 0x24,
        INCF = 0x28,
        DECFSZ = 0x2C,
        RRCF = 0x30,
        RLCF = 0x34,
        SWAPF = 0x38,
        INCFSZ = 0x3C,
        RRNCF = 0x40,
        RLNCF = 0x44,
        INFSNZ = 0x48,
        DCFSNZ = 0x4C,
        MOVF = 0x50,
        SUBFWB = 0x54,
        SUBWFB = 0x58,
        SUBWF = 0x5C,
    }

    enum PicAluInstruction2 : byte
    {
        // 0 1 1 0 opcode:3 a f:8 (ALU operations, do not write to W)
        Prefix = 0x60,
        PrefixMask = 0xF0,
        OpCodeMask = 0x0E,

        CPFSLT = 0x0,
        CPFSEQ = 0x2,
        CPFSGT = 0x4,
        TSTFSZ = 0x6,
        SETF = 0x8,
        CLRF = 0xA,
        NEGF = 0xC,
        MOVWF = 0xE,
    }

    enum PicBraRCallInstruction : byte
    {
        // 1 1 0 1 op addr:3  addr:8
        Prefix = 0xD0,
        PrefixMask = 0xF0,
        OpCodeMask = 0x08,

        OffsetHiMask = 0x07,
        OffsetHiSignMask = 0x04,

        BRA = 0x00,
        RCALL = 0x80,
    }

    class PicInstructionBuf
    {
        public int InstructionLength;
        public byte HiByte;
        public byte LoByte;

        public PicInstructionKind InstrucitonKind;

        // PicInstructionKind.Misc
        public PicMiscInstruction MiscInstruction;

        // PicInstructionKind.Literal
        public PicLiteralInstruction LiteralInstruction;
        public byte LiteralInstuctionLiteral;

        // PicInstructionKind.Alu
        public PicAluInstruction AluInstruction;
        public byte AluInstructionFileReg;

        // PicInstructionKind.Alu2
        public PicAluInstruction2 AluInstruction2;
        public byte AluInstruction2FileReg;

        // PicInstructionKind.BraRCall
        public PicBraRCallInstruction BraRCallInstruction;
        public int BraRCallInstructionOffset;
    }

    class PicInstructionDecoder
    {
        private readonly byte[] data;

        public PicInstructionDecoder(byte[] data)
        {
            this.data = data;
        }

        public bool DecodeAt(PicInstructionBuf buf, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return false;
            }

            buf.HiByte = data[offset + 1];
            buf.LoByte = data[offset];

            if (buf.HiByte == 0x00)
            {
                buf.InstrucitonKind = PicInstructionKind.Misc;
                buf.MiscInstruction = (PicMiscInstruction)buf.LoByte;

                // TODO: check

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & (byte)PicLiteralInstruction.PrefixMask) == (byte) PicLiteralInstruction.Prefix)
            {
                buf.InstrucitonKind = PicInstructionKind.Literal;
                buf.LiteralInstruction = (PicLiteralInstruction)(buf.HiByte & (byte)PicLiteralInstruction.OpCodeMask);
                buf.LiteralInstuctionLiteral = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & (byte)PicAluInstruction2.PrefixMask) == (byte)PicAluInstruction2.Prefix)
            {
                buf.InstrucitonKind = PicInstructionKind.Alu2;
                buf.AluInstruction2 = (PicAluInstruction2)(buf.HiByte & (byte)PicAluInstruction2.OpCodeMask);
                buf.AluInstruction2FileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & (byte)PicAluInstruction.PrefixMask) == (byte)PicAluInstruction.Prefix)
            {
                buf.InstrucitonKind = PicInstructionKind.Alu;
                buf.AluInstruction = (PicAluInstruction)(buf.HiByte & (byte)PicAluInstruction.OpCodeMask);
                buf.AluInstructionFileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & (byte)PicBraRCallInstruction.PrefixMask) == (byte)PicBraRCallInstruction.Prefix)
            {
                buf.InstrucitonKind = PicInstructionKind.BraRCall;
                buf.BraRCallInstruction = (PicBraRCallInstruction)(buf.HiByte & (byte)PicBraRCallInstruction.OpCodeMask);

                buf.BraRCallInstructionOffset = (byte)(buf.HiByte & (byte)PicBraRCallInstruction.OffsetHiMask);

                buf.InstructionLength = 2;
                return true;
            }

            buf.InstrucitonKind = PicInstructionKind.Unknown;
            buf.InstructionLength = 2;
            return true;
        }
    }
}
