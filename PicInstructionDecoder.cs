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

        public int CallGotoAddr
        {
            get
            {
                return ((exHi & 0xf) << 16) | (exLo << 8) | loByte;
            }
        }

        public int MovffSource
        {
            get
            {
                return ((hiByte & 0xf) << 8) | loByte;
            }
        }

        public int MovffDest
        {
            get
            {
                return ((exHi & 0xf) << 8) | exLo;
            }
        }

        public int ConditionalOffset
        {
            get
            {
                sbyte tmp = (sbyte)loByte;
                return tmp;
            }
        }

        public int BitOpBit
        {
            get
            {
                return (hiByte & 0xE) >> 1;
            }
        }

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

        public CallReturnOpMode CallMode
        {
            get
            {
                return CallReturnMode(hiByte);
            }
        }

        public CallReturnOpMode ReturnRetfieMode
        {
            get
            {
                return CallReturnMode(loByte);
            }
        }

        public byte UnkHi
        {
            get { return hiByte; }
        }

        public byte UnkLo
        {
            get { return loByte; }
        }

        public byte NopExHi
        {
            get { return hiByte; }
        }

        public byte NopExLo
        {
            get { return loByte; }
        }

        public int Bank
        {
            get { return loByte & 0x0f; }
        }

        public byte Literal
        {
            get { return loByte; }
        }

        public byte FileReg
        {
            get { return loByte; }
        }

        public int FsrN => loByte >> 6;
        public int FsrLiteral => loByte & 0x3f;

        public int LfsrFSR => (loByte >> 4) & 3;
        public int LfsrLiteral => ((loByte & 0xf) << 8) | exLo;

        public int MovssSrc => loByte & 0x7f;
        public int MovssDst => exLo & 0x7f;

        public int MovsfFileReg
        {
            get
            {
                return ((exHi & 0xf) << 8) | exLo;
            }
        }
    }

    enum DecodeResult
    {
        Success = 0,
        SuccessLong = 1,
        FetchLong = -1,
    }

    enum PicInstrucitonType
    {
        // Special values
        Misc = -1,
        SubLfsr = -2,
        AddLfsr = -3,
        Movsf_Movss = -4,

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

        MOVFF, // int source, int dest
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
        MOVSF, // int src, int dst
        MOVSS, // int src, int dst

        CALL, // int addr, CallReturnOpMode mode
        LFSR, // int f, int k
        GOTO, // int addr

        NOPEX, // byte hiByte, byte loByte
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

        static readonly PicInstrucitonType[] s_hiMap = new PicInstrucitonType[256];

        private static void Inst(byte bits, byte mask, PicInstrucitonType instr)
        {
            MapInst(s_hiMap, bits, mask, instr);
        }

        private static void Inst(byte bits, PicInstrucitonType instr)
        {
            MapInst(s_hiMap, bits, 0b_1111_1111, instr);
        }

        static PicInstructionDecoder()
        {
            Inst(0b_0000_0000, PicInstrucitonType.Misc);

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

            Inst(0b_1110_1000, PicInstrucitonType.AddLfsr);
            Inst(0b_1110_1001, PicInstrucitonType.SubLfsr);

            Inst(0b_1110_1010, PicInstrucitonType.PUSHL);

            Inst(0b_1110_1011, PicInstrucitonType.Movsf_Movss);

            Inst(0b_1110_1100, 0b_1111_1110, PicInstrucitonType.CALL);

            Inst(0b_1110_1110, PicInstrucitonType.LFSR);

            Inst(0b_1110_1111, PicInstrucitonType.GOTO);

            Inst(0b_1111_0000, 0b_1111_0000, PicInstrucitonType.NOPEX);
        }

        public PicInstructionDecoder(IPicInstructionExecutor e)
        {
            this.e = e;
        }

        public DecodeResult Decode(PicInstructionBuf buf)
        {
            switch (s_hiMap[buf.NopExHi])
            {
                // Miscellaneous instructions
                // 0b_0000_0000 opcode

                case PicInstrucitonType.Misc:
                    switch (buf.NopExLo)
                    {
                        case 0b_0000_0000: e.NOP(); return DecodeResult.Success;
                        case 0b_0000_0011: e.SLEEP(); return DecodeResult.Success;
                        case 0b_0000_0100: e.CLRWDT(); return DecodeResult.Success;
                        case 0b_0000_0101: e.PUSH(); return DecodeResult.Success;
                        case 0b_0000_0110: e.POP(); return DecodeResult.Success;
                        case 0b_0000_0111: e.DAW(); return DecodeResult.Success;

                        case 0b_0000_1000:
                        case 0b_0000_1001:
                        case 0b_0000_1010:
                        case 0b_0000_1011: e.TBLRD(buf.TableMode); return DecodeResult.Success;
                        case 0b_0000_1100:
                        case 0b_0000_1101:
                        case 0b_0000_1110:
                        case 0b_0000_1111: e.TBLWT(buf.TableMode); return DecodeResult.Success;

                        case 0b_0001_0000:
                        case 0b_0001_0001: e.RETFIE(buf.ReturnRetfieMode); return DecodeResult.Success;
                        case 0b_0001_0010:
                        case 0b_0001_0011: e.RETURN(buf.ReturnRetfieMode); return DecodeResult.Success;

                        case 0b_0001_0100: e.CALLW(); return DecodeResult.Success;

                        case 0b_0001_0101: e.EMULEN(); return DecodeResult.Success;
                        case 0b_0001_0110: e.EMULDIS(); return DecodeResult.Success;

                        case 0b_1111_0000:
                        case 0b_1111_0001:
                        case 0b_1111_0011:
                        case 0b_1111_0010:
                        case 0b_1111_0100:
                        case 0b_1111_0101:
                        case 0b_1111_0111:
                        case 0b_1111_0110:
                        case 0b_1111_1000:
                        case 0b_1111_1001:
                        case 0b_1111_1011:
                        case 0b_1111_1010:
                        case 0b_1111_1100:
                        case 0b_1111_1101:
                        case 0b_1111_1111:
                        case 0b_1111_1110: e.RESET(); return DecodeResult.Success;

                        default:
                            e.Unknown(buf.UnkHi, buf.UnkLo);
                            return DecodeResult.Success;
                    }

                // MOVLB
                case PicInstrucitonType.MOVLB:
                    //if ((buf.loByte & 0xf0) != 0)
                    //    goto unknown;
                    e.MOVLB(buf.Bank);
                    return DecodeResult.Success;

                // Literal operations: W ← OP(k,W)
                // 0b_0000_1ooo k

                case PicInstrucitonType.SUBLW: e.SUBLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.IORLW: e.IORLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.XORLW: e.XORLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.ANDLW: e.ANDLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.RETLW: e.RETLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.MULLW: e.MULLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.MOVLW: e.MOVLW(buf.Literal); return DecodeResult.Success;
                case PicInstrucitonType.ADDLW: e.ADDLW(buf.Literal); return DecodeResult.Success;

                // register ALU operations: dest ← OP(f,W)
                // 0b_0ooo_ooda 
                case PicInstrucitonType.MULWF: e.MULWF(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.DECF: e.DECF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.IORWF: e.IORWF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.ANDWF: e.ANDWF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.XORWF: e.XORWF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.COMF: e.COMF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.ADDWFC: e.ADDWFC(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.ADDWF: e.ADDWF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.INCF: e.INCF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.DECFSZ: e.DECFSZ(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.RRCF: e.RRCF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.RLCF: e.RLCF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.SWAPF: e.SWAPF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.INCFSZ: e.INCFSZ(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.RRNCF: e.RRNCF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.RLNCF: e.RLNCF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.INFSNZ: e.INFSNZ(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.DCFSNZ: e.DCFSNZ(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.MOVF: e.MOVF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.SUBFWB: e.SUBFWB(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.SUBWFB: e.SUBWFB(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.SUBWF: e.SUBWF(buf.FileReg, buf.Destination, buf.Access); return DecodeResult.Success;

                // register ALU operations, do not write to W
                // 0b_0110_oooa f
                case PicInstrucitonType.CPFSLT: e.CPFSLT(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.CPFSEQ: e.CPFSEQ(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.CPFSGT: e.CPFSGT(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.TSTFSZ: e.TSTFSZ(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.SETF: e.SETF(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.CLRF: e.CLRF(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.NEGF: e.NEGF(buf.FileReg, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.MOVWF: e.MOVWF(buf.FileReg, buf.Access); return DecodeResult.Success;

                // 0b_0111_bbba BTG f,b,a Toggle bit b of f
                case PicInstrucitonType.BTG: e.BTG(buf.FileReg, buf.BitOpBit, buf.Access); return DecodeResult.Success;

                // register Bit operations
                // 0b_10oo_bbba 
                case PicInstrucitonType.BSF: e.BSF(buf.FileReg, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.BCF: e.BCF(buf.FileReg, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.BTFSS: e.BTFSS(buf.FileReg, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case PicInstrucitonType.BTFSC: e.BTFSC(buf.FileReg, buf.BitOpBit, buf.Access); return DecodeResult.Success;

                // MOVFF
                case PicInstrucitonType.MOVFF:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        e.MOVFF(buf.MovffSource, buf.MovffDest);
                        return DecodeResult.SuccessLong;
                    }

                // BRA n
                case PicInstrucitonType.BRA: e.BRA(buf.BraRCallOffset); return DecodeResult.Success;
                case PicInstrucitonType.RCALL: e.RCALL(buf.BraRCallOffset); return DecodeResult.Success;

                // Conditional branch (to PC+2n)
                case PicInstrucitonType.BZ: e.BZ(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BNZ: e.BNZ(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BC: e.BC(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BNC: e.BNC(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BOV: e.BOV(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BNOV: e.BNOV(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BN: e.BN(buf.ConditionalOffset); return DecodeResult.Success;
                case PicInstrucitonType.BNN: e.BNN(buf.ConditionalOffset); return DecodeResult.Success;

                case PicInstrucitonType.AddLfsr:
                    {
                        if (buf.FsrN == 3)
                        {
                            e.ADDULNK(buf.FsrLiteral);
                        }
                        else
                        {
                            e.ADDFSR(buf.FsrN, buf.FsrLiteral);
                        }

                        return DecodeResult.Success;
                    }

                case PicInstrucitonType.SubLfsr:
                    {
                        if (buf.FsrN == 3)
                        {
                            e.SUBULNK(buf.FsrLiteral);
                        }
                        else
                        {
                            e.SUBFSR(buf.FsrN, buf.FsrLiteral);
                        }

                        return DecodeResult.Success;
                    }

                case PicInstrucitonType.PUSHL: e.PUSHL(buf.Literal); return DecodeResult.Success;

                case PicInstrucitonType.Movsf_Movss:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        if ((buf.UnkLo & 0x80) == 0)
                        {
                            e.MOVSF(buf.MovssSrc, buf.MovsfFileReg);
                        }
                        else
                        {
                            e.MOVSS(buf.MovssSrc, buf.MovssDst);
                        }

                        return DecodeResult.SuccessLong;
                    }

                // CALL k
                case PicInstrucitonType.CALL:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if ((exHi & 0xf0) != 0xf0)
                        //    goto unknown;
                        e.CALL(buf.CallGotoAddr, buf.CallMode);
                        return DecodeResult.SuccessLong;
                    }

                // LFSR n
                case PicInstrucitonType.LFSR:
                    {
                        //if ((buf.loByte & 0xC0) != 0)
                        //    goto unknown;

                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if (exHi  != 0xf0)
                        //    goto unknown;
                        e.LFSR(buf.LfsrFSR, buf.LfsrLiteral);
                        return DecodeResult.SuccessLong;
                    }

                // GOTO k
                case PicInstrucitonType.GOTO:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if ((exHi & 0xf0) != 0xf0)
                        //    goto unknown;
                        e.GOTO(buf.CallGotoAddr);
                        return DecodeResult.SuccessLong;
                    }

                case PicInstrucitonType.NOPEX: e.NOPEX(buf.NopExHi, buf.NopExLo); return DecodeResult.Success;

                case PicInstrucitonType.Unknown:
                default:
                    e.Unknown(buf.UnkHi, buf.UnkLo);
                    return DecodeResult.Success;
            }
        }
    }
}
