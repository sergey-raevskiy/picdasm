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
        PreIncrement = 2,
        PostDecrement = 3,
    }

    enum CallReturnOpMode
    {
        Fast = 0x00,
        Slow = 0x01,
    }

    interface IPicInstructionExecutor
    {
        // Miscellaneous instructions
        void NOP();
        void SLEEP();
        void CLRWDT();
        //void PUSH();
        void POP();
        //void DAW();

        void TBLRD(TableOpMode mode);
        void TBLWT(TableOpMode mode);

        void RETFIE(CallReturnOpMode mode);
        void RETURN(CallReturnOpMode mode);

        void CALLW();

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

        void CALL(int addr, CallReturnOpMode mode);
        void LFSR(int f, int k);
        void GOTO(int addr);

        void NOPEX(byte hiByte, byte loByte);

        void Unknown(byte hiByte, byte loByte);
    }

    class PicInstructionDecoder
    {
        private IPicInstructionFetcher fetcher;
        private IPicInstructionExecutor e;

        public PicInstructionDecoder(IPicInstructionFetcher fetcher,
                                     IPicInstructionExecutor e)
        {
            this.fetcher = fetcher;
            this.e = e;
        }

        private static AccessMode Access(byte hiByte)
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

        private static DestinationMode Destination(byte hiByte)
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

        private static TableOpMode TableMode(byte loByte)
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

        private static int BraRCallOffset(byte hiByte, byte loByte)
        {
            short tmp = (short)((hiByte << 8) | loByte);
            tmp <<= 5;
            tmp >>= 5;
            return tmp;
        }

        private static int CallGotoAddr(byte loByte, byte exHi, byte exLo)
        {
            return ((exHi & 0xf) << 16) | (exLo << 8) | loByte;
        }

        private int MovffSource(byte hiByte, byte loByte)
        {
            return ((hiByte & 0xf) << 8) | loByte;
        }

        private int MovffDest(byte exHi, byte exLo)
        {
            return ((exHi & 0xf) << 8) | exLo;
        }

        private int ConditionalOffset(byte loByte)
        {
            sbyte tmp = (sbyte)loByte;
            return tmp;
        }

        private int BitOpBit(byte hiByte)
        {
            return (hiByte & 6) >> 1;
        }

        static CallReturnOpMode CallReturnMode(byte iByte)
        {
            byte mode = (byte) (iByte & 1);
            switch(mode)
            {
                case (byte)CallReturnOpMode.Fast:
                case (byte)CallReturnOpMode.Slow:
                    return (CallReturnOpMode)mode;

                default:
                    throw new InvalidOperationException();
            }
        }

        public int DecodeAt()
        {
            byte hiByte;
            byte loByte;

            fetcher.FetchInstruciton(out hiByte, out loByte);

            switch (hiByte)
            {
                // Miscellaneous instructions
                // 0b_0000_0000 opcode

                case 0b_0000_0000:
                    switch (loByte)
                    {
                        case 0b_0000_0000: e.NOP(); return 2;
                        case 0b_0000_0011: e.SLEEP(); return 2;
                        case 0b_0000_0100: e.CLRWDT(); return 2;
                        //case 0b_0000_0101: p.PUSH(); return 2;
                        case 0b_0000_0110: e.POP(); return 2;
                        //case 0b_0000_0111: p.DAW(); return 2;

                        case 0b_0000_1000:
                        case 0b_0000_1001:
                        case 0b_0000_1010:
                        case 0b_0000_1011: e.TBLRD(TableMode(loByte)); return 2;
                        case 0b_0000_1100:
                        case 0b_0000_1101:
                        case 0b_0000_1110:
                        case 0b_0000_1111: e.TBLWT(TableMode(loByte)); return 2;

                        case 0b_0001_0000:
                        case 0b_0001_0001: e.RETFIE(CallReturnMode(loByte)); return 2;
                        case 0b_0001_0010:
                        case 0b_0001_0011: e.RETURN(CallReturnMode(loByte)); return 2;

                        case 0b_0001_0100: e.CALLW(); return 2;

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
                        case 0b_1111_1110: e.RESET(); return 2;

                        default: goto unknown;
                    }

                // MOVLB
                case 0b_0000_0001:
                    //if ((loByte & 0xf0) != 0)
                    //    goto unknown;
                    e.MOVLB(loByte & 0x0f);
                    return 2;

                // Literal operations: W ← OP(k,W)
                // 0b_0000_1ooo k

                case 0b_0000_1000: e.SUBLW(loByte); return 2;
                case 0b_0000_1001: e.IORLW(loByte); return 2;
                case 0b_0000_1010: e.XORLW(loByte); return 2;
                case 0b_0000_1011: e.ANDLW(loByte); return 2;
                case 0b_0000_1100: e.RETLW(loByte); return 2;
                case 0b_0000_1101: e.MULLW(loByte); return 2;
                case 0b_0000_1110: e.MOVLW(loByte); return 2;
                case 0b_0000_1111: e.ADDLW(loByte); return 2;

                // register ALU operations: dest ← OP(f,W)
                // 0b_0ooo_ooda 
                case 0b_0000_0010: // no MULWF with d = 0
                case 0b_0000_0011: e.MULWF(loByte, Access(hiByte)); return 2;
                case 0b_0000_0100:
                case 0b_0000_0101:
                case 0b_0000_0110:
                case 0b_0000_0111: e.DECF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_0000:
                case 0b_0001_0001:
                case 0b_0001_0010:
                case 0b_0001_0011: e.IORWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_0100:
                case 0b_0001_0101:
                case 0b_0001_0110:
                case 0b_0001_0111: e.ANDWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_1000:
                case 0b_0001_1001:
                case 0b_0001_1010:
                case 0b_0001_1011: e.XORWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_1100:
                case 0b_0001_1101:
                case 0b_0001_1110:
                case 0b_0001_1111: e.COMF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_0000:
                case 0b_0010_0001:
                case 0b_0010_0010:
                case 0b_0010_0011: e.ADDWFC(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_0100:
                case 0b_0010_0101:
                case 0b_0010_0110:
                case 0b_0010_0111: e.ADDWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_1000:
                case 0b_0010_1001:
                case 0b_0010_1010:
                case 0b_0010_1011: e.INCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_1100:
                case 0b_0010_1101:
                case 0b_0010_1110:
                case 0b_0010_1111: e.DECFSZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_0000:
                case 0b_0011_0001:
                case 0b_0011_0010:
                case 0b_0011_0011: e.RRCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_0100:
                case 0b_0011_0101:
                case 0b_0011_0110:
                case 0b_0011_0111: e.RLCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_1000:
                case 0b_0011_1001:
                case 0b_0011_1010:
                case 0b_0011_1011: e.SWAPF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_1100:
                case 0b_0011_1101:
                case 0b_0011_1110:
                case 0b_0011_1111: e.INCFSZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_0000:
                case 0b_0100_0001:
                case 0b_0100_0010:
                case 0b_0100_0011: e.RRNCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_0100:
                case 0b_0100_0101:
                case 0b_0100_0110:
                case 0b_0100_0111: e.RLNCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_1000:
                case 0b_0100_1001:
                case 0b_0100_1010:
                case 0b_0100_1011: e.INFSNZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_1100:
                case 0b_0100_1101:
                case 0b_0100_1110:
                case 0b_0100_1111: e.DCFSNZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_0000:
                case 0b_0101_0001:
                case 0b_0101_0010:
                case 0b_0101_0011: e.MOVF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_0100:
                case 0b_0101_0101:
                case 0b_0101_0110:
                case 0b_0101_0111: e.SUBFWB(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_1000:
                case 0b_0101_1001:
                case 0b_0101_1010:
                case 0b_0101_1011: e.SUBWFB(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_1100:
                case 0b_0101_1101:
                case 0b_0101_1110:
                case 0b_0101_1111: e.SUBWF(loByte, Destination(hiByte), Access(hiByte)); return 2;

                // register ALU operations, do not write to W
                // 0b_0110_oooa f
                case 0b_0110_0000:
                case 0b_0110_0001: e.CPFSLT(loByte, Access(hiByte)); return 2;
                case 0b_0110_0010:
                case 0b_0110_0011: e.CPFSEQ(loByte, Access(hiByte)); return 2;
                case 0b_0110_0100:
                case 0b_0110_0101: e.CPFSGT(loByte, Access(hiByte)); return 2;
                case 0b_0110_0110:
                case 0b_0110_0111: e.TSTFSZ(loByte, Access(hiByte)); return 2;
                case 0b_0110_1000:
                case 0b_0110_1001: e.SETF(loByte, Access(hiByte)); return 2;
                case 0b_0110_1010:
                case 0b_0110_1011: e.CLRF(loByte, Access(hiByte)); return 2;
                case 0b_0110_1100:
                case 0b_0110_1101: e.NEGF(loByte, Access(hiByte)); return 2;
                case 0b_0110_1110:
                case 0b_0110_1111: e.MOVWF(loByte, Access(hiByte)); return 2;

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
                case 0b_0111_1111: e.BTG(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;

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
                case 0b_1000_1111: e.BSF(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1001_1111: e.BCF(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1010_1111: e.BTFSS(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1011_1111: e.BTFSC(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;

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
                        byte exHi;
                        byte exLo;
                        fetcher.FetchInstruciton(out exHi, out exLo);

                        e.MOVFF(MovffSource(hiByte, loByte), MovffDest(exHi, exLo));
                        return 4;
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
                case 0b_1101_0111: e.BRA(BraRCallOffset(hiByte, loByte)); return 2;

                // 0b_1101_1nnn
                case 0b_1101_1000:
                case 0b_1101_1001:
                case 0b_1101_1010:
                case 0b_1101_1011:
                case 0b_1101_1100:
                case 0b_1101_1101:
                case 0b_1101_1110:
                case 0b_1101_1111: e.RCALL(BraRCallOffset(hiByte, loByte)); return 2;

                // Conditional branch (to PC+2n)
                // 0b_1110_0ccc n 
                case 0b_1110_0000: e.BZ(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0001: e.BNZ(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0010: e.BC(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0011: e.BNC(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0100: e.BOV(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0101: e.BNOV(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0110: e.BN(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0111: e.BNN(ConditionalOffset(loByte)); return 2;

                // CALL k
                case 0b_1110_1100:
                case 0b_1110_1101:
                    {
                        byte exHi;
                        byte exLo;
                        fetcher.FetchInstruciton(out exHi, out exLo);

                        if ((exHi & 0xf0) != 0xf0)
                            goto unknown;
                        e.CALL(CallGotoAddr(loByte, exHi, exLo), CallReturnMode(hiByte));
                        return 4;
                    }

                // LFSR n
                case 0b_1110_1110:
                    {
                        if ((loByte & 0xC0) != 0)
                            goto unknown;

                        byte exHi;
                        byte exLo;
                        fetcher.FetchInstruciton(out exHi, out exLo);

                        if (exHi  != 0xf0)
                            goto unknown;
                        e.LFSR((loByte >> 4) & 3, exLo | ((loByte & 0xf) << 8));
                        return 4;
                    }

                // GOTO k
                case 0b_1110_1111:
                    {
                        byte exHi;
                        byte exLo;
                        fetcher.FetchInstruciton(out exHi, out exLo);

                        if ((exHi & 0xf0) != 0xf0)
                            goto unknown;
                        e.GOTO(CallGotoAddr(loByte, exHi, exLo));
                        return 4;
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
                case 0b_1111_1111: e.NOPEX(hiByte, loByte); return 2;

                default: goto unknown;
            }

            unknown:
            e.Unknown(hiByte, loByte);
            return 2;
        }
    }
}
