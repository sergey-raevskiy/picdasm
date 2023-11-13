using System;

namespace picdasm
{
    // no MULWF with d = 0

    enum AccessMode
    {
        Access = 0x00,
        Banked = 0x01,
    }

    enum DestinationMode
    {
        W = 0x00,
        F = 0x02,
    }

    enum TableOpMode
    {
        None = 0,
        PostIncrement = 1,
        PostDecrement = 2,
        PreIncrement = 3,
    }

    enum CallReturnOpMode
    {
        Fast = 0x00,
        Slow = 0x01,
    }

    class PicInstructionBuf
    {
        public bool isLong;
        public byte hiByte;
        public byte loByte;
        public byte exHi;
        public byte exLo;

        public override string ToString()
        {
            string result;

            if (isLong)
            {
                result = string.Format("{0:X2}{1:X2} {2:X2}{3:X2}", hiByte, loByte, exHi, exLo);
            }
            else
            {
                result = string.Format("{0:X2}{1:X2}", hiByte, loByte);
            }

            return result;
        }

        //public PicInstrucitonType Instruction;

        public AccessMode Access
        {
            get
            {
                byte access = (byte)(hiByte & 0x01);
                switch (access)
                {
                    case (byte)AccessMode.Access:
                    case (byte)AccessMode.Banked:
                        return (AccessMode)access;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public DestinationMode Destination
        {
            get
            {
                byte dest = (byte)(hiByte & 0x02);
                switch (dest)
                {
                    case (byte)DestinationMode.W:
                    case (byte)DestinationMode.F:
                        return (DestinationMode)dest;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public TableOpMode TableMode
        {
            get
            {
                byte mode = (byte)(loByte & 0x3);
                switch (mode)
                {
                    case (byte)TableOpMode.None:
                    case (byte)TableOpMode.PostIncrement:
                    case (byte)TableOpMode.PostDecrement:
                    case (byte)TableOpMode.PreIncrement:
                        return (TableOpMode)mode;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public int BraRCallOffset
        {
            get
            {
                short tmp = (short)((hiByte << 8) | loByte);
                tmp <<= 5;
                tmp >>= 5;
                return tmp;
            }
        }

        public int CallGotoAddr => ((exHi & 0xf) << 16) | (exLo << 8) | loByte;

        public int MovffSource => ((hiByte & 0xf) << 8) | loByte;
        public int MovffDest => ((exHi & 0xf) << 8) | exLo;

        public int ConditionalOffset => (sbyte)loByte;
        public int BitOpBit => (hiByte & 0xE) >> 1;

        private static CallReturnOpMode CallReturnMode(byte iByte)
        {
            byte mode = (byte)(iByte & 1);
            switch (mode)
            {
                case (byte)CallReturnOpMode.Fast:
                case (byte)CallReturnOpMode.Slow:
                    return (CallReturnOpMode)mode;

                default:
                    throw new InvalidOperationException();
            }
        }

        public CallReturnOpMode CallMode => CallReturnMode(hiByte);
        public CallReturnOpMode ReturnRetfieMode => CallReturnMode(loByte);

        public byte UnkHi => hiByte;
        public byte UnkLo => loByte;

        public byte NopExHi => hiByte;
        public byte NopExLo => loByte;

        public int Bank => loByte & 0x0f;

        public byte Literal => loByte;
        public byte FileReg => loByte;

        public int FsrN => loByte >> 6;
        public int FsrLiteral => loByte & 0x3f;

        public int LfsrFSR => (loByte >> 4) & 3;
        public int LfsrLiteral => ((loByte & 0xf) << 8) | exLo;

        public int MovssSrc => loByte & 0x7f;
        public int MovssDst => exLo & 0x7f;

        public int MovsfFileReg => ((exHi & 0xf) << 8) | exLo;
    }

    enum PicInstrucitonType
    {
        // Special values
        Misc = -1,
        SubFsrOp = -2,
        AddFsrOp = -3,
        MovFsrOp = -4,

        Unknown = 0, // byte hiByte, byte loByte

        // Miscellaneous instructions
        NOP,
        SLEEP,
        CLRWDT,
        PUSH,
        POP,
        DAW,

        TBLRD, // TableOpMode mode
        TBLWT, // TableOpMode mode

        RETFIE, // CallReturnOpMode mode 
        RETURN, // CallReturnOpMode mode 

        CALLW,

        EMULEN,
        EMULDIS,

        RESET,

        MOVLB, // int literal

        // Literal operations: W ← OP(k,W)
        SUBLW, // byte literal
        IORLW, // byte literal
        XORLW, // byte literal
        ANDLW, // byte literal
        RETLW, // byte literal
        MULLW, // byte literal
        MOVLW, // byte literal
        ADDLW, // byte literal

        // register ALU operations: dest ← OP(f,W)
        MULWF, // byte addr, AccessMode access
        DECF, // byte addr, DestinationMode dest, AccessMode access
        IORWF, // byte addr, DestinationMode dest, AccessMode access
        ANDWF, // byte addr, DestinationMode dest, AccessMode access
        XORWF, // byte addr, DestinationMode dest, AccessMode access
        COMF, // byte addr, DestinationMode dest, AccessMode access
        ADDWFC, // byte addr, DestinationMode dest, AccessMode access
        ADDWF, // byte addr, DestinationMode dest, AccessMode access
        INCF, // byte addr, DestinationMode dest, AccessMode access
        DECFSZ, // byte addr, DestinationMode dest, AccessMode access
        RRCF, // byte addr, DestinationMode dest, AccessMode access
        RLCF, // byte addr, DestinationMode dest, AccessMode access
        SWAPF, // byte addr, DestinationMode dest, AccessMode access
        INCFSZ, // byte addr, DestinationMode dest, AccessMode access
        RRNCF, // byte addr, DestinationMode dest, AccessMode access
        RLNCF, // byte addr, DestinationMode dest, AccessMode access
        INFSNZ, // byte addr, DestinationMode dest, AccessMode access
        DCFSNZ, // byte addr, DestinationMode dest, AccessMode access
        MOVF, // byte addr, DestinationMode dest, AccessMode access
        SUBFWB, // byte addr, DestinationMode dest, AccessMode access
        SUBWFB, // byte addr, DestinationMode dest, AccessMode access
        SUBWF, // byte addr, DestinationMode dest, AccessMode access

        // register ALU operations, do not write to W
        CPFSLT, // byte addr, AccessMode access
        CPFSEQ, // byte addr, AccessMode access
        CPFSGT, // byte addr, AccessMode access
        TSTFSZ, // byte addr, AccessMode access
        SETF, // byte addr, AccessMode access
        CLRF, // byte addr, AccessMode access
        NEGF, // byte addr, AccessMode access
        MOVWF, // byte addr, AccessMode access

        // Toggle bit b of f
        BTG, // byte addr, int bit, AccessMode access

        // register Bit operations
        // 0b_10oo_bbba 
        BSF, // byte addr, int bit, AccessMode access
        BCF, // byte addr, int bit, AccessMode access
        BTFSS, // byte addr, int bit, AccessMode access
        BTFSC, // byte addr, int bit, AccessMode access

        BRA, // int offset
        RCALL, // int offset

        // Conditional branch (to PC+2n)
        // 0b_1110_0ccc n 
        BZ, // int off
        BNZ, // int off
        BC, // int off
        BNC, // int off
        BOV, // int off
        BNOV, // int off
        BN, // int off
        BNN, // int off

        ADDFSR, // int n, int k
        ADDULNK, // int k
        SUBFSR, // int n, int k
        SUBULNK, // int k
        PUSHL, // byte l

        NOPEX, // byte hiByte, byte loByte

        LongStart,

        MOVFF, // int source, int dest
        MOVSF, // int src, int dst
        MOVSS, // int src, int dst

        CALL, // int addr, CallReturnOpMode mode
        LFSR, // int f, int k
        GOTO, // int addr
    }

    class PicInstructionDecoder
    {
        public IPicInstructionExecutor e;

        private static void MapInst(PicInstrucitonType[] map,
                                    byte bits, byte mask,
                                    PicInstrucitonType instr)
        {
            for (int i = 0; i < 256; i++)
            {
                if ((i & mask) != bits)
                    continue;

                if (map[i] != PicInstrucitonType.Unknown)
                    throw new InvalidOperationException("Map conflict");

                map[i] = instr;
            }
        }

        static readonly PicInstrucitonType[] s_instMap = new PicInstrucitonType[256];

        private static void Inst(byte bits, byte mask, PicInstrucitonType instr)
        {
            MapInst(s_instMap, bits, mask, instr);
        }

        private static void Inst(byte bits, PicInstrucitonType instr)
        {
            MapInst(s_instMap, bits, 0b_1111_1111, instr);
        }

        static readonly PicInstrucitonType[] s_miscMap = new PicInstrucitonType[256];

        private static void MiscInst(byte bits, byte mask, PicInstrucitonType instr)
        {
            MapInst(s_miscMap, bits, mask, instr);
        }

        private static void MiscInst(byte bits, PicInstrucitonType instr)
        {
            MapInst(s_miscMap, bits, 0b_1111_1111, instr);
        }

        static PicInstructionDecoder()
        {
            // Miscellaneous instructions
            // 0b_0000_0000 opcode
            Inst(0b_0000_0000, PicInstrucitonType.Misc);

            MiscInst(0b_0000_0000, PicInstrucitonType.NOP);
            MiscInst(0b_0000_0011, PicInstrucitonType.SLEEP);
            MiscInst(0b_0000_0100, PicInstrucitonType.CLRWDT);
            MiscInst(0b_0000_0101, PicInstrucitonType.PUSH);
            MiscInst(0b_0000_0110, PicInstrucitonType.POP);
            MiscInst(0b_0000_0111, PicInstrucitonType.DAW);

            MiscInst(0b_0000_1000, 0b_1111_1100, PicInstrucitonType.TBLRD);
            MiscInst(0b_0000_1100, 0b_1111_1100, PicInstrucitonType.TBLWT);

            MiscInst(0b_0001_0000, 0b_1111_1110, PicInstrucitonType.RETFIE);
            MiscInst(0b_0001_0010, 0b_1111_1110, PicInstrucitonType.RETURN);

            MiscInst(0b_0001_0100, PicInstrucitonType.CALLW);

            MiscInst(0b_0001_0101, PicInstrucitonType.EMULEN);
            MiscInst(0b_0001_0110, PicInstrucitonType.EMULDIS);

            MiscInst(0b_1111_0000, 0b_1111_0000, PicInstrucitonType.RESET);

            Inst(0b_0000_0001, PicInstrucitonType.MOVLB);

            // Literal operations: W ← OP(k,W)
            // 0b_0000_1ooo k
            Inst(0b_0000_1000, PicInstrucitonType.SUBLW);
            Inst(0b_0000_1001, PicInstrucitonType.IORLW);
            Inst(0b_0000_1010, PicInstrucitonType.XORLW);
            Inst(0b_0000_1011, PicInstrucitonType.ANDLW);
            Inst(0b_0000_1100, PicInstrucitonType.RETLW);
            Inst(0b_0000_1101, PicInstrucitonType.MULLW);
            Inst(0b_0000_1110, PicInstrucitonType.MOVLW);
            Inst(0b_0000_1111, PicInstrucitonType.ADDLW);

            // register ALU operations: dest ← OP(f,W)
            // 0b_0ooo_ooda 
            // no MULWF with d = 0
            Inst(0b_0000_0010, 0b_1111_1110, PicInstrucitonType.MULWF);
            Inst(0b_0000_0100, 0b_1111_1100, PicInstrucitonType.DECF);
            Inst(0b_0001_0000, 0b_1111_1100, PicInstrucitonType.IORWF);
            Inst(0b_0001_0100, 0b_1111_1100, PicInstrucitonType.ANDWF);
            Inst(0b_0001_1000, 0b_1111_1100, PicInstrucitonType.XORWF);
            Inst(0b_0001_1100, 0b_1111_1100, PicInstrucitonType.COMF);
            Inst(0b_0010_0000, 0b_1111_1100, PicInstrucitonType.ADDWFC);
            Inst(0b_0010_0100, 0b_1111_1100, PicInstrucitonType.ADDWF);
            Inst(0b_0010_1000, 0b_1111_1100, PicInstrucitonType.INCF);
            Inst(0b_0010_1100, 0b_1111_1100, PicInstrucitonType.DECFSZ);
            Inst(0b_0011_0000, 0b_1111_1100, PicInstrucitonType.RRCF);
            Inst(0b_0011_0100, 0b_1111_1100, PicInstrucitonType.RLCF);
            Inst(0b_0011_1000, 0b_1111_1100, PicInstrucitonType.SWAPF);
            Inst(0b_0011_1100, 0b_1111_1100, PicInstrucitonType.INCFSZ);
            Inst(0b_0100_0000, 0b_1111_1100, PicInstrucitonType.RRNCF);
            Inst(0b_0100_0100, 0b_1111_1100, PicInstrucitonType.RLNCF);
            Inst(0b_0100_1000, 0b_1111_1100, PicInstrucitonType.INFSNZ);
            Inst(0b_0100_1100, 0b_1111_1100, PicInstrucitonType.DCFSNZ);
            Inst(0b_0101_0000, 0b_1111_1100, PicInstrucitonType.MOVF);
            Inst(0b_0101_0100, 0b_1111_1100, PicInstrucitonType.SUBFWB);
            Inst(0b_0101_1000, 0b_1111_1100, PicInstrucitonType.SUBWFB);
            Inst(0b_0101_1100, 0b_1111_1100, PicInstrucitonType.SUBWF);

            // register ALU operations, do not write to W
            // 0b_0110_oooa f
            Inst(0b_0110_0000, 0b_1111_1110, PicInstrucitonType.CPFSLT);
            Inst(0b_0110_0010, 0b_1111_1110, PicInstrucitonType.CPFSEQ);
            Inst(0b_0110_0100, 0b_1111_1110, PicInstrucitonType.CPFSGT);
            Inst(0b_0110_0110, 0b_1111_1110, PicInstrucitonType.TSTFSZ);
            Inst(0b_0110_1000, 0b_1111_1110, PicInstrucitonType.SETF);
            Inst(0b_0110_1010, 0b_1111_1110, PicInstrucitonType.CLRF);
            Inst(0b_0110_1100, 0b_1111_1110, PicInstrucitonType.NEGF);
            Inst(0b_0110_1110, 0b_1111_1110, PicInstrucitonType.MOVWF);

            // register Bit operations
            Inst(0b_0111_0000, 0b_1111_0000, PicInstrucitonType.BTG);
            Inst(0b_1000_0000, 0b_1111_0000, PicInstrucitonType.BSF);
            Inst(0b_1001_0000, 0b_1111_0000, PicInstrucitonType.BCF);
            Inst(0b_1010_0000, 0b_1111_0000, PicInstrucitonType.BTFSS);
            Inst(0b_1011_0000, 0b_1111_0000, PicInstrucitonType.BTFSC);

            Inst(0b_1100_0000, 0b_1111_0000, PicInstrucitonType.MOVFF);

            Inst(0b_1101_0000, 0b_1111_1000, PicInstrucitonType.BRA);
            Inst(0b_1101_1000, 0b_1111_1000, PicInstrucitonType.RCALL);

            // Conditional branch (to PC+2n)
            Inst(0b_1110_0000, PicInstrucitonType.BZ);
            Inst(0b_1110_0001, PicInstrucitonType.BNZ);
            Inst(0b_1110_0010, PicInstrucitonType.BC);
            Inst(0b_1110_0011, PicInstrucitonType.BNC);
            Inst(0b_1110_0100, PicInstrucitonType.BOV);
            Inst(0b_1110_0101, PicInstrucitonType.BNOV);
            Inst(0b_1110_0110, PicInstrucitonType.BN);
            Inst(0b_1110_0111, PicInstrucitonType.BNN);

            Inst(0b_1110_1000, PicInstrucitonType.AddFsrOp);
            Inst(0b_1110_1001, PicInstrucitonType.SubFsrOp);

            Inst(0b_1110_1010, PicInstrucitonType.PUSHL);

            Inst(0b_1110_1011, PicInstrucitonType.MovFsrOp);

            Inst(0b_1110_1100, 0b_1111_1110, PicInstrucitonType.CALL);

            Inst(0b_1110_1110, PicInstrucitonType.LFSR);

            Inst(0b_1110_1111, PicInstrucitonType.GOTO);

            Inst(0b_1111_0000, 0b_1111_0000, PicInstrucitonType.NOPEX);
        }

        public PicInstructionDecoder(IPicInstructionExecutor e)
        {
            this.e = e;
        }

        public PicInstrucitonType Decode(PicInstructionBuf buf)
        {
            var inst = s_instMap[buf.NopExHi];

            switch (inst)
            {
                case PicInstrucitonType.Misc:
                    inst = s_miscMap[buf.loByte];
                    break;

                case PicInstrucitonType.AddFsrOp:
                    if ((buf.loByte & 0xC0) == 0xC0)
                        inst = PicInstrucitonType.ADDULNK;
                    else
                        inst = PicInstrucitonType.ADDFSR;
                    break;

                case PicInstrucitonType.SubFsrOp:
                    if ((buf.loByte & 0xC0) == 0xC0)
                        inst = PicInstrucitonType.SUBULNK;
                    else
                        inst = PicInstrucitonType.SUBFSR;
                    break;

                case PicInstrucitonType.MovFsrOp:
                    if ((buf.loByte & 0x80) == 0)
                        inst = PicInstrucitonType.MOVSF;
                    else
                        inst = PicInstrucitonType.MOVSS;
                    break;

                default:
                    if (inst < PicInstrucitonType.Unknown)
                        throw new NotImplementedException(string.Format("Unknown special type ({0})", inst));
                    break;
            }

            return inst;
        }
    }
}
