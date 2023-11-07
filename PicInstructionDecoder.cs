using System;

namespace picdasm
{
    enum PicInstructionKind
    {
        Unknown,
        Misc,
        TBLRD,
        TBLWR,
        RETFIE,
        RETURN,
        Literal,
        Alu,
        Alu2,
        BRA,
        RCALL,
        GOTO,
        MOVFF,
        ConditionalBranch,
        LFSR,
        BitInstruction,
        CALL,
        CALLFast,
        NOPEx,
    }

    enum PicMiscInstruction : byte
    {
        NOP = 0x00,
        SLEEP = 0x03,
        CLRWDT = 0x04,
        PUSH = 0x05,
        POP = 0x06,
        DAW = 0x07,

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

    struct PicRetInstuction
    {
        public bool Fast;
    }

    struct PicLiteralInstruction
    {
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

    struct Piс20bitAbsInstruciton
    {
        public int Addr;

        public void Init(byte loByte, byte exHi, byte exLo)
        {
            Addr = loByte;
            Addr |= ((exHi & 0xf) << 16) | (exLo << 8);
            Addr <<= 1;
        }
    }

    struct PicMOVFFInstuction
    {
        public int Source;
        public int Dest;

        public void Init(byte hiByte, byte loByte, byte exHi, byte exLo)
        {
            Source = ((hiByte & 0xf) << 8) | loByte;
            Dest = ((exHi & 0xf) << 8) | exLo;
        }
    }

    struct PicConditionalBranchInstruction
    {
        public enum BranchOp
        {
            BZ = 0x0,
            BNZ = 0x1,
            BC = 0x2,
            BNC = 0x3,
            BOV = 0x4,
            BNOV = 0x5,
            BN = 0x6,
            BNN = 0x7,
        }

        public BranchOp OpCode;
        public int Offset;
    }

    struct PicLFSRInstruction
    {
        public int LfsrN;
        public uint Literal;
    }

    struct PicBitInstruction
    {
        public enum BitOp : byte
        {
            BSF = 0b_0000_0000,
            BCF = 0b_0001_0000,
            BTFSS = 0b_0010_0000,
            BTFSC = 0b_0011_0000,

            BTG = 0b_0111_0000,
        }

        public BitOp OpCode;
    }

    struct PicNOPExInstruction
    {
        int Arg;
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
        public PicRetInstuction RETURN;
        public PicRetInstuction RETFIE;
        public PicLiteralInstruction LiteralInstruction;
        public PicAluInstruction AluInstruction;
        public PicAluInstruction2 AluInstruction2;
        public PicBraRCallInstruction Bra;
        public PicBraRCallInstruction RCall;
        public Piс20bitAbsInstruciton GOTO;
        public PicMOVFFInstuction MOVFF;
        public PicConditionalBranchInstruction ConditionalBranch;
        public PicLFSRInstruction LFSR;
        public PicBitInstruction BitInstruction;
        public Piс20bitAbsInstruciton CALL;
        public Piс20bitAbsInstruciton CALLFast;
        public PicNOPExInstruction NOPEx;
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

            if (buf.HiByte == 0b_0000_0000)
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
                    buf.InstructionLength = 2;
                    return true;
                }
                else if ((buf.LoByte & 0xFC) == 0xC)
                {
                    buf.InstrucitonKind = PicInstructionKind.TBLWR;
                    buf.TBLWR.Mode = (PicTblInstructionMode)(buf.LoByte & 0x3);
                    buf.InstructionLength = 2;
                    return true;
                }
                else if ((buf.LoByte & 0xFE) == 0x10)
                {
                    buf.InstrucitonKind = PicInstructionKind.RETFIE;
                    buf.RETFIE.Fast = (buf.LoByte & 0x1) != 0;
                    buf.InstructionLength = 2;
                    return true;
                }
                else if ((buf.LoByte & 0xFE) == 0x12)
                {
                    buf.InstrucitonKind = PicInstructionKind.RETURN;
                    buf.RETURN.Fast = (buf.LoByte & 0x1) != 0;
                    buf.InstructionLength = 2;
                    return true;
                }
            }
            else if ((byte)(buf.HiByte & 0b_1111_1000) == 0b_0000_1000)
            {
                // 0 0 0 0  1 opcode:3 | literal:8
                buf.InstrucitonKind = PicInstructionKind.Literal;
                buf.LiteralInstruction.OpCode = (PicLiteralInstruction.LiteralOp)(buf.HiByte & 0b_0000_0111);
                buf.LiteralInstruction.Literal = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1111_0000) == 0b_0110_0000)
            {
                // (ALU operations, do not write to W)
                // 0 1 1 0 opcode:3 a | f:8
                buf.InstrucitonKind = PicInstructionKind.Alu2;
                buf.AluInstruction2.OpCode = (PicAluInstruction2.Alu2Op)(buf.HiByte & 0b_0000_1110);
                buf.AluInstruction2.FileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1000_0000) == 0b_0000_0000)
            {
                // 0 opcode:5 d a | f:8
                buf.InstrucitonKind = PicInstructionKind.Alu;
                buf.AluInstruction.OpCode = (PicAluInstruction.AluOp)(buf.HiByte & 0b_0111_1100);
                buf.AluInstruction.FileReg = buf.LoByte;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1111_0000) == 0b_1101_0000)
            {
                if ((buf.HiByte & 0b_0000_1000) != 0)
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
            else if ((byte)(buf.HiByte & 0b_1111_1000) == 0b_1110_0000)
            {
                buf.InstrucitonKind = PicInstructionKind.ConditionalBranch;
                buf.ConditionalBranch.OpCode = (PicConditionalBranchInstruction.BranchOp)(buf.HiByte & 0b_0000_0111);
                buf.ConditionalBranch.Offset = buf.LoByte;
                if ((buf.LoByte & 0x80) != 0)
                    buf.ConditionalBranch.Offset |= unchecked((int)0xFFFFFF00);
                buf.ConditionalBranch.Offset <<= 1;

                buf.InstructionLength = 2;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1111_0000) == 0b_1100_0000)
            {
                if (offset + 4 > data.Length)
                    return false;

                buf.InstrucitonKind = PicInstructionKind.MOVFF;
                buf.MOVFF.Init(buf.HiByte, buf.LoByte, data[offset + 3], data[offset + 2]);
                buf.InstructionLength = 4;
                return true;
            }
            else if (buf.HiByte == 0b_1110_1111)
            {
                if (offset + 4 > data.Length)
                    return false;

                buf.InstrucitonKind = PicInstructionKind.GOTO;
                buf.GOTO.Init(buf.LoByte, data[offset + 3], data[offset + 2]);
                buf.InstructionLength = 4;
                return true;
            }
            else if (buf.HiByte == 0b_1110_1110 &&
                     (byte)(buf.LoByte & 0b_1100_0000) == 0b_0000_0000)
            {
                if (offset + 4 > data.Length)
                    return false;

                buf.InstrucitonKind = PicInstructionKind.LFSR;
                buf.LFSR.LfsrN = (buf.LoByte >> 4) & 3;
                buf.LFSR.Literal = data[offset + 2] | (uint)((buf.LoByte & 0xf) << 8);
                buf.InstructionLength = 4;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1100_0000) == 0b_1000_0000)
            {
                buf.InstrucitonKind = PicInstructionKind.BitInstruction;
                buf.BitInstruction.OpCode = ((PicBitInstruction.BitOp)(buf.HiByte & 0b_0011_0000));
                buf.InstructionLength = 2;
                return true;
            }
            else if (buf.HiByte == 0b_1110_1100)
            {
                if (offset + 4 > data.Length)
                    return false;

                buf.InstrucitonKind = PicInstructionKind.CALL;
                buf.CALL.Init(buf.LoByte, data[offset + 3], data[offset + 2]);
                buf.InstructionLength = 4;
                return true;
            }
            else if ((byte)(buf.HiByte & 0b_1111_0000) == 0b_1111_0000)
            {
                buf.InstrucitonKind = PicInstructionKind.NOPEx;
                buf.InstructionLength = 2;
                return true;
            }

            buf.InstrucitonKind = PicInstructionKind.Unknown;
            buf.InstructionLength = 2;
            return true;
        }
    }
}
