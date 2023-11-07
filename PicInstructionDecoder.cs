using System;

namespace picdasm
{
    enum PicInstructionKind
    {
        Unknown,
        Misc,
        TBLRD,
        TBLWR,
        Literal,
        Alu,
        Alu2,
        BRA,
        RCALL,
    }

    enum PicMiscInstruction : byte
    {
        NOP = 0x00,
        SLEEP = 0x03,
        CLRWDT = 0x04,
        PUSH = 0x05,
        POP = 0x06,
        DAW = 0x07,

        RETFIE = 0x10,
        RETFIEFast = 0x11,
        RETURN = 0x12,
        RETURNFast = 0x13,
        CALLW = 0x14,
        RESET = 0xFF,
    }

    enum PicTblInstructionMode : byte
    {
        None = 0x00,
        PostIncrement = 0x01,
        PostDecrement = 0x02,
        PreIncrement = 0x03,
    }

    struct PicTblInstuction
    {
        public PicTblInstructionMode Mode;
    }

    struct PicLiteralInstruction
    {
        // 0 0 0 0  1 opcode:3 literal:8
        public enum LiteralOp
        {
            SUBLW = 0x0,
            IORLW = 0x1,
            XORLW = 0x2,
            ANDLW = 0x3,
            RETLW = 0x4,
            MULLW = 0x5,
            MOVLW = 0x6,
            ADDLW = 0x7,
        }

        public LiteralOp OpCode;
        public byte Literal;
    }

    struct PicAluInstruction
    {
        // 0 opcode:5 d a f:8
        public enum AluOp : byte
        {
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

        public AluOp OpCode;
        public byte FileReg;
    }

    struct PicAluInstruction2
    {
        public enum Alu2Op
        {
            CPFSLT = 0x0,
            CPFSEQ = 0x2,
            CPFSGT = 0x4,
            TSTFSZ = 0x6,
            SETF = 0x8,
            CLRF = 0xA,
            NEGF = 0xC,
            MOVWF = 0xE,
        }

        public Alu2Op OpCode;
        public byte FileReg;
    }

    struct PicBraRCallInstruction
    {
        // 1 1 0 1 op addr:3  addr:8
        public int Addr;

        public void Init(byte hiByte, byte loByte)
        {
            Addr = ((hiByte & 0x7) << 8) | loByte;

            if ((hiByte & 0x4) != 0)
            {
                Addr |= unchecked((int)(0xFFFF8000));
            }

            Addr <<= 1;
        }
    }

    class PicInstructionBuf
    {
        public int InstructionLength;
        public byte HiByte;
        public byte LoByte;

        public PicInstructionKind InstrucitonKind;

        public PicMiscInstruction MiscInstruction;
        public PicTblInstuction TBLRD;
        public PicTblInstuction TBLWR;
        public PicLiteralInstruction LiteralInstruction;
        public PicAluInstruction AluInstruction;
        public PicAluInstruction2 AluInstruction2;
        public PicBraRCallInstruction Bra;
        public PicBraRCallInstruction RCall;
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

                switch (buf.LoByte)
                {
                    case (byte)PicMiscInstruction.NOP:
                    case (byte)PicMiscInstruction.SLEEP:
                    case (byte)PicMiscInstruction.CLRWDT:
                    case (byte)PicMiscInstruction.PUSH:
                    case (byte)PicMiscInstruction.POP:
                    case (byte)PicMiscInstruction.DAW:
                    case (byte)PicMiscInstruction.RETFIE:
                    case (byte)PicMiscInstruction.RETFIEFast:
                    case (byte)PicMiscInstruction.RETURN:
                    case (byte)PicMiscInstruction.RETURNFast:
                    case (byte)PicMiscInstruction.CALLW:
                    case (byte)PicMiscInstruction.RESET:
                        buf.MiscInstruction = (PicMiscInstruction)buf.LoByte;
                        buf.InstructionLength = 2;
                        return true;

                    default:
                        break;
                }

                if ((buf.LoByte & 0xFC) == 0x8)
                {
                    buf.InstrucitonKind = PicInstructionKind.TBLRD;
                    buf.TBLRD.Mode = (PicTblInstructionMode)(buf.LoByte & 0x3);
                }
                else if ((buf.LoByte & 0xFC) == 0xC)
                {
                    buf.InstrucitonKind = PicInstructionKind.TBLWR;
                    buf.TBLWR.Mode = (PicTblInstructionMode)(buf.LoByte & 0x3);
                }
            }
            else if ((byte)(buf.HiByte & 0xF8) == 0x08)
            {
                buf.InstrucitonKind = PicInstructionKind.Literal;
                buf.LiteralInstruction.OpCode = (PicLiteralInstruction.LiteralOp)(buf.HiByte & 0x07);
                buf.LiteralInstruction.Literal = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0xF0) == 0x60)
            {
                // 0 1 1 0 opcode:3 a f:8 (ALU operations, do not write to W)
                buf.InstrucitonKind = PicInstructionKind.Alu2;
                buf.AluInstruction2.OpCode = (PicAluInstruction2.Alu2Op)(buf.HiByte & 0x0E);
                buf.AluInstruction2.FileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0x80) == 0x00)
            {
                buf.InstrucitonKind = PicInstructionKind.Alu;
                buf.AluInstruction.OpCode = (PicAluInstruction.AluOp)(buf.HiByte & 0x7C);
                buf.AluInstruction.FileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0xF0) == 0xD0)
            {
                if ((buf.HiByte & 0x08) != 0)
                {
                    buf.InstrucitonKind = PicInstructionKind.RCALL;
                    buf.RCall.Init(buf.HiByte, buf.LoByte);
                }
                else
                {
                    buf.InstrucitonKind = PicInstructionKind.BRA;
                    buf.Bra.Init(buf.HiByte, buf.LoByte);
                }

                buf.InstructionLength = 2;
                return true;
            }

            buf.InstrucitonKind = PicInstructionKind.Unknown;
            buf.InstructionLength = 2;
            return true;
        }
    }
}
