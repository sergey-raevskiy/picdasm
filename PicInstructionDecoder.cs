using System;

namespace picdasm
{
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

    interface IPicInstructionExecutor
    {
        // HACK
        void SetPc(int pc);

        // Miscellaneous instructions
        void NOP();
        void SLEEP();
        void CLRWDT();
        void PUSH();
        void POP();
        void DAW();

        void TBLRD(TableOpMode mode);
        void TBLWT(TableOpMode mode);

        void RETFIE(CallReturnOpMode mode);
        void RETURN(CallReturnOpMode mode);

        void CALLW();

        void EMULEN();
        void EMULDIS();

        void RESET();

        void MOVLB(int literal);

        // Literal operations: W ← OP(k,W)
        void SUBLW(byte literal);
        void IORLW(byte literal);
        void XORLW(byte literal);
        void ANDLW(byte literal);
        void RETLW(byte literal);
        void MULLW(byte literal);
        void MOVLW(byte literal);
        void ADDLW(byte literal);

        // register ALU operations: dest ← OP(f,W)
        void MULWF(byte addr, AccessMode access);
        void DECF(byte addr, DestinationMode dest, AccessMode access);
        void IORWF(byte addr, DestinationMode dest, AccessMode access);
        void ANDWF(byte addr, DestinationMode dest, AccessMode access);
        void XORWF(byte addr, DestinationMode dest, AccessMode access);
        void COMF(byte addr, DestinationMode dest, AccessMode access);
        void ADDWFC(byte addr, DestinationMode dest, AccessMode access);
        void ADDWF(byte addr, DestinationMode dest, AccessMode access);
        void INCF(byte addr, DestinationMode dest, AccessMode access);
        void DECFSZ(byte addr, DestinationMode dest, AccessMode access);
        void RRCF(byte addr, DestinationMode dest, AccessMode access);
        void RLCF(byte addr, DestinationMode dest, AccessMode access);
        void SWAPF(byte addr, DestinationMode dest, AccessMode access);
        void INCFSZ(byte addr, DestinationMode dest, AccessMode access);
        void RRNCF(byte addr, DestinationMode dest, AccessMode access);
        void RLNCF(byte addr, DestinationMode dest, AccessMode access);
        void INFSNZ(byte addr, DestinationMode dest, AccessMode access);
        void DCFSNZ(byte addr, DestinationMode dest, AccessMode access);
        void MOVF(byte addr, DestinationMode dest, AccessMode access);
        void SUBFWB(byte addr, DestinationMode dest, AccessMode access);
        void SUBWFB(byte addr, DestinationMode dest, AccessMode access);
        void SUBWF(byte addr, DestinationMode dest, AccessMode access);

        // register ALU operations, do not write to W
        void CPFSLT(byte addr, AccessMode access);
        void CPFSEQ(byte addr, AccessMode access);
        void CPFSGT(byte addr, AccessMode access);
        void TSTFSZ(byte addr, AccessMode access);
        void SETF(byte addr, AccessMode access);
        void CLRF(byte addr, AccessMode access);
        void NEGF(byte addr, AccessMode access);
        void MOVWF(byte addr, AccessMode access);

        // Toggle bit b of f
        void BTG(byte addr, int bit, AccessMode access);

        // register Bit operations
        // 0b_10oo_bbba 
        void BSF(byte addr, int bit, AccessMode access);
        void BCF(byte addr, int bit, AccessMode access);
        void BTFSS(byte addr, int bit, AccessMode access);
        void BTFSC(byte addr, int bit, AccessMode access);

        void MOVFF(int source, int dest);
        void BRA(int offset);
        void RCALL(int offset);

        // Conditional branch (to PC+2n)
        // 0b_1110_0ccc n 
        void BZ(int off);
        void BNZ(int off);
        void BC(int off);
        void BNC(int off);
        void BOV(int off);
        void BNOV(int off);
        void BN(int off);
        void BNN(int off);

        void ADDFSR(int n, int k);
        void ADDULNK(int k);
        void SUBFSR(int n, int k);
        void SUBULNK(int k);
        void PUSHL(byte l);
        void MOVSF(int src, int dst);
        void MOVSS(int src, int dst);

        void CALL(int addr, CallReturnOpMode mode);
        void LFSR(int f, int k);
        void GOTO(int addr);

        void NOPEX(byte hiByte, byte loByte);

        void Unknown(byte hiByte, byte loByte);
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
    }

    enum DecodeResult
    {
        Success = 0,
        SuccessLong = 1,
        FetchLong = -1,
    }

    class PicInstructionDecoder
    {
        public IPicInstructionExecutor e;

        public PicInstructionDecoder(IPicInstructionExecutor e)
        {
            this.e = e;
        }

        public DecodeResult Decode(PicInstructionBuf buf)
        {
            switch (buf.hiByte)
            {
                // Miscellaneous instructions
                // 0b_0000_0000 opcode

                case 0b_0000_0000:
                    switch (buf.loByte)
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

                        default: goto unknown;
                    }

                // MOVLB
                case 0b_0000_0001:
                    //if ((buf.loByte & 0xf0) != 0)
                    //    goto unknown;
                    e.MOVLB(buf.loByte & 0x0f);
                    return DecodeResult.Success;

                // Literal operations: W ← OP(k,W)
                // 0b_0000_1ooo k

                case 0b_0000_1000: e.SUBLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1001: e.IORLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1010: e.XORLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1011: e.ANDLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1100: e.RETLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1101: e.MULLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1110: e.MOVLW(buf.loByte); return DecodeResult.Success;
                case 0b_0000_1111: e.ADDLW(buf.loByte); return DecodeResult.Success;

                // register ALU operations: dest ← OP(f,W)
                // 0b_0ooo_ooda 
                case 0b_0000_0010: // no MULWF with d = 0
                case 0b_0000_0011: e.MULWF(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0000_0100:
                case 0b_0000_0101:
                case 0b_0000_0110:
                case 0b_0000_0111: e.DECF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0001_0000:
                case 0b_0001_0001:
                case 0b_0001_0010:
                case 0b_0001_0011: e.IORWF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0001_0100:
                case 0b_0001_0101:
                case 0b_0001_0110:
                case 0b_0001_0111: e.ANDWF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0001_1000:
                case 0b_0001_1001:
                case 0b_0001_1010:
                case 0b_0001_1011: e.XORWF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0001_1100:
                case 0b_0001_1101:
                case 0b_0001_1110:
                case 0b_0001_1111: e.COMF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0010_0000:
                case 0b_0010_0001:
                case 0b_0010_0010:
                case 0b_0010_0011: e.ADDWFC(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0010_0100:
                case 0b_0010_0101:
                case 0b_0010_0110:
                case 0b_0010_0111: e.ADDWF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0010_1000:
                case 0b_0010_1001:
                case 0b_0010_1010:
                case 0b_0010_1011: e.INCF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0010_1100:
                case 0b_0010_1101:
                case 0b_0010_1110:
                case 0b_0010_1111: e.DECFSZ(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0011_0000:
                case 0b_0011_0001:
                case 0b_0011_0010:
                case 0b_0011_0011: e.RRCF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0011_0100:
                case 0b_0011_0101:
                case 0b_0011_0110:
                case 0b_0011_0111: e.RLCF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0011_1000:
                case 0b_0011_1001:
                case 0b_0011_1010:
                case 0b_0011_1011: e.SWAPF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0011_1100:
                case 0b_0011_1101:
                case 0b_0011_1110:
                case 0b_0011_1111: e.INCFSZ(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0100_0000:
                case 0b_0100_0001:
                case 0b_0100_0010:
                case 0b_0100_0011: e.RRNCF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0100_0100:
                case 0b_0100_0101:
                case 0b_0100_0110:
                case 0b_0100_0111: e.RLNCF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0100_1000:
                case 0b_0100_1001:
                case 0b_0100_1010:
                case 0b_0100_1011: e.INFSNZ(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0100_1100:
                case 0b_0100_1101:
                case 0b_0100_1110:
                case 0b_0100_1111: e.DCFSNZ(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0101_0000:
                case 0b_0101_0001:
                case 0b_0101_0010:
                case 0b_0101_0011: e.MOVF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0101_0100:
                case 0b_0101_0101:
                case 0b_0101_0110:
                case 0b_0101_0111: e.SUBFWB(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0101_1000:
                case 0b_0101_1001:
                case 0b_0101_1010:
                case 0b_0101_1011: e.SUBWFB(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;
                case 0b_0101_1100:
                case 0b_0101_1101:
                case 0b_0101_1110:
                case 0b_0101_1111: e.SUBWF(buf.loByte, buf.Destination, buf.Access); return DecodeResult.Success;

                // register ALU operations, do not write to W
                // 0b_0110_oooa f
                case 0b_0110_0000:
                case 0b_0110_0001: e.CPFSLT(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_0010:
                case 0b_0110_0011: e.CPFSEQ(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_0100:
                case 0b_0110_0101: e.CPFSGT(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_0110:
                case 0b_0110_0111: e.TSTFSZ(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_1000:
                case 0b_0110_1001: e.SETF(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_1010:
                case 0b_0110_1011: e.CLRF(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_1100:
                case 0b_0110_1101: e.NEGF(buf.loByte, buf.Access); return DecodeResult.Success;
                case 0b_0110_1110:
                case 0b_0110_1111: e.MOVWF(buf.loByte, buf.Access); return DecodeResult.Success;

                // 0b_0111_bbba BTG f,b,a Toggle bit b of f
                case 0b_0111_0000:
                case 0b_0111_0001:
                case 0b_0111_0010:
                case 0b_0111_0011:
                case 0b_0111_0100:
                case 0b_0111_0101:
                case 0b_0111_0110:
                case 0b_0111_0111:
                case 0b_0111_1000:
                case 0b_0111_1001:
                case 0b_0111_1010:
                case 0b_0111_1011:
                case 0b_0111_1100:
                case 0b_0111_1101:
                case 0b_0111_1110:
                case 0b_0111_1111: e.BTG(buf.loByte, buf.BitOpBit, buf.Access); return DecodeResult.Success;

                // register Bit operations
                // 0b_10oo_bbba 
                case 0b_1000_0000:
                case 0b_1000_0001:
                case 0b_1000_0010:
                case 0b_1000_0011:
                case 0b_1000_0100:
                case 0b_1000_0101:
                case 0b_1000_0110:
                case 0b_1000_0111:
                case 0b_1000_1000:
                case 0b_1000_1001:
                case 0b_1000_1010:
                case 0b_1000_1011:
                case 0b_1000_1100:
                case 0b_1000_1101:
                case 0b_1000_1110:
                case 0b_1000_1111: e.BSF(buf.loByte, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case 0b_1001_0000:
                case 0b_1001_0001:
                case 0b_1001_0010:
                case 0b_1001_0011:
                case 0b_1001_0100:
                case 0b_1001_0101:
                case 0b_1001_0110:
                case 0b_1001_0111:
                case 0b_1001_1000:
                case 0b_1001_1001:
                case 0b_1001_1010:
                case 0b_1001_1011:
                case 0b_1001_1100:
                case 0b_1001_1101:
                case 0b_1001_1110:
                case 0b_1001_1111: e.BCF(buf.loByte, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case 0b_1010_0000:
                case 0b_1010_0001:
                case 0b_1010_0010:
                case 0b_1010_0011:
                case 0b_1010_0100:
                case 0b_1010_0101:
                case 0b_1010_0110:
                case 0b_1010_0111:
                case 0b_1010_1000:
                case 0b_1010_1001:
                case 0b_1010_1010:
                case 0b_1010_1011:
                case 0b_1010_1100:
                case 0b_1010_1101:
                case 0b_1010_1110:
                case 0b_1010_1111: e.BTFSS(buf.loByte, buf.BitOpBit, buf.Access); return DecodeResult.Success;
                case 0b_1011_0000:
                case 0b_1011_0001:
                case 0b_1011_0010:
                case 0b_1011_0011:
                case 0b_1011_0100:
                case 0b_1011_0101:
                case 0b_1011_0110:
                case 0b_1011_0111:
                case 0b_1011_1000:
                case 0b_1011_1001:
                case 0b_1011_1010:
                case 0b_1011_1011:
                case 0b_1011_1100:
                case 0b_1011_1101:
                case 0b_1011_1110:
                case 0b_1011_1111: e.BTFSC(buf.loByte, buf.BitOpBit, buf.Access); return DecodeResult.Success;

                // MOVFF
                case 0b_1100_0000:
                case 0b_1100_0001:
                case 0b_1100_0010:
                case 0b_1100_0011:
                case 0b_1100_0100:
                case 0b_1100_0101:
                case 0b_1100_0110:
                case 0b_1100_0111:
                case 0b_1100_1000:
                case 0b_1100_1001:
                case 0b_1100_1010:
                case 0b_1100_1011:
                case 0b_1100_1100:
                case 0b_1100_1101:
                case 0b_1100_1110:
                case 0b_1100_1111:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        e.MOVFF(buf.MovffSource, buf.MovffDest);
                        return DecodeResult.SuccessLong;
                    }

                // BRA n
                // 0b_1101_0nnn
                case 0b_1101_0000:
                case 0b_1101_0001:
                case 0b_1101_0010:
                case 0b_1101_0011:
                case 0b_1101_0100:
                case 0b_1101_0101:
                case 0b_1101_0110:
                case 0b_1101_0111: e.BRA(buf.BraRCallOffset); return DecodeResult.Success;

                // 0b_1101_1nnn
                case 0b_1101_1000:
                case 0b_1101_1001:
                case 0b_1101_1010:
                case 0b_1101_1011:
                case 0b_1101_1100:
                case 0b_1101_1101:
                case 0b_1101_1110:
                case 0b_1101_1111: e.RCALL(buf.BraRCallOffset); return DecodeResult.Success;

                // Conditional branch (to PC+2n)
                // 0b_1110_0ccc n 
                case 0b_1110_0000: e.BZ(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0001: e.BNZ(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0010: e.BC(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0011: e.BNC(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0100: e.BOV(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0101: e.BNOV(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0110: e.BN(buf.ConditionalOffset); return DecodeResult.Success;
                case 0b_1110_0111: e.BNN(buf.ConditionalOffset); return DecodeResult.Success;

                case 0b_1110_1000:
                    {
                        int n = buf.loByte >> 6;

                        if (n  == 3)
                        {
                            e.ADDULNK(buf.loByte & 0x3f);
                        }
                        else
                        {
                            e.ADDFSR(n, buf.loByte & 0x3f);
                        }

                        return DecodeResult.Success;
                    }

                case 0b_1110_1001:
                    {
                        int n = buf.loByte >> 6;

                        if (n == 3)
                        {
                            e.SUBULNK(buf.loByte & 0x3f);
                        }
                        else
                        {
                            e.SUBFSR(n, buf.loByte & 0x3f);
                        }

                        return DecodeResult.Success;
                    }

                case 0b_1110_1010: e.PUSHL(buf.loByte); return DecodeResult.Success;

                case 0b_1110_1011:
                    {
                        byte src = (byte)(buf.loByte & 0x7f);
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        if ((buf.loByte & 0x80) == 0)
                        {
                            e.MOVSF(src, ((buf.exHi & 0xf) << 8) | buf.exLo);
                        }
                        else
                        {
                            e.MOVSS(src, buf.exLo & 0x7f);
                        }

                        return DecodeResult.SuccessLong;
                    }

                // CALL k
                case 0b_1110_1100:
                case 0b_1110_1101:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if ((exHi & 0xf0) != 0xf0)
                        //    goto unknown;
                        e.CALL(buf.CallGotoAddr, buf.CallMode);
                        return DecodeResult.SuccessLong;
                    }

                // LFSR n
                case 0b_1110_1110:
                    {
                        //if ((buf.loByte & 0xC0) != 0)
                        //    goto unknown;

                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if (exHi  != 0xf0)
                        //    goto unknown;
                        e.LFSR((buf.loByte >> 4) & 3, buf.exLo | ((buf.loByte & 0xf) << 8));
                        return DecodeResult.SuccessLong;
                    }

                // GOTO k
                case 0b_1110_1111:
                    {
                        if (!buf.isLong) return DecodeResult.FetchLong;

                        //if ((exHi & 0xf0) != 0xf0)
                        //    goto unknown;
                        e.GOTO(buf.CallGotoAddr);
                        return DecodeResult.SuccessLong;
                    }

                case 0b_1111_0000:
                case 0b_1111_0001:
                case 0b_1111_0010:
                case 0b_1111_0011:
                case 0b_1111_0100:
                case 0b_1111_0101:
                case 0b_1111_0110:
                case 0b_1111_0111:
                case 0b_1111_1000:
                case 0b_1111_1001:
                case 0b_1111_1010:
                case 0b_1111_1011:
                case 0b_1111_1100:
                case 0b_1111_1101:
                case 0b_1111_1110:
                case 0b_1111_1111: e.NOPEX(buf.hiByte, buf.loByte); return DecodeResult.Success;

                default: goto unknown;
            }

            unknown:
            e.Unknown(buf.hiByte, buf.loByte);
            return DecodeResult.Success;
        }
    }
}
