﻿using System;

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

    interface IPicInstructionProcessor
    {
        // Miscellaneous instructions
        void NOP();
        //void SLEEP();
        void CLRWDT();
        //void PUSH();
        void POP();
        //void DAW();

        void TBLRD(TableOpMode mode);
        void TBLWT(TableOpMode mode);

        void RETFIE(CallReturnOpMode mode);
        void RETURN(CallReturnOpMode mode);

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
        //void NEGF(byte addr, AccessMode access);
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
        private readonly byte[] data;
        private IPicInstructionProcessor p;

        public PicInstructionDecoder(byte[] data, IPicInstructionProcessor p)
        {
            this.data = data;
            this.p = p;
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
            return -1;
        }

        private static int CallGotoAddr(byte loByte, byte exHi, byte exLo)
        {
            return -1;
        }

        private int MovffSource(byte hiByte, byte loByte)
        {
            return -1;
        }

        private int MovffDest(byte exHi, byte exLo)
        {
            return -1;
        }

        private int ConditionalOffset(byte loByte)
        {
            return -1;
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

        public int DecodeAt(int offset)
        {
            if (data.Length < offset + 2)
                return 0;

            byte hiByte = data[offset + 1];
            byte loByte = data[offset];

            switch (hiByte)
            {
                // Miscellaneous instructions
                // 0b_0000_0000 opcode

                case 0b_0000_0000:
                    switch (loByte)
                    {
                        case 0b_0000_0000: p.NOP(); return 2;
                        //case 0b_0000_0011: p.SLEEP(); return 2;
                        case 0b_0000_0100: p.CLRWDT(); return 2;
                        //case 0b_0000_0101: p.PUSH(); return 2;
                        case 0b_0000_0110: p.POP(); return 2;
                        //case 0b_0000_0111: p.DAW(); return 2;

                        case 0b_0000_1000:
                        case 0b_0000_1001:
                        case 0b_0000_1010:
                        case 0b_0000_1011: p.TBLRD(TableMode(loByte)); return 2;
                        case 0b_0000_1100:
                        case 0b_0000_1101:
                        case 0b_0000_1110:
                        case 0b_0000_1111: p.TBLWT(TableMode(loByte)); return 2;

                        case 0b_0001_0000:
                        case 0b_0001_0001: p.RETFIE(CallReturnMode(loByte)); return 2;
                        case 0b_0001_0010:
                        case 0b_0001_0011: p.RETURN(CallReturnMode(loByte)); return 2;

                        case 0b_1111_1111: p.RESET(); return 2;

                        default: goto unknown;
                    }

                // MOVLB
                case 0b_0000_0001:
                    if ((loByte & 0xf0) != 0)
                        goto unknown;
                    p.MOVLB(loByte & 0x0f);
                    return 2;

                // Literal operations: W ← OP(k,W)
                // 0b_0000_1ooo k

                case 0b_0000_1000: p.SUBLW(loByte); return 2;
                case 0b_0000_1001: p.IORLW(loByte); return 2;
                case 0b_0000_1010: p.XORLW(loByte); return 2;
                case 0b_0000_1011: p.ANDLW(loByte); return 2;
                case 0b_0000_1100: p.RETLW(loByte); return 2;
                case 0b_0000_1101: p.MULLW(loByte); return 2;
                case 0b_0000_1110: p.MOVLW(loByte); return 2;
                case 0b_0000_1111: p.ADDLW(loByte); return 2;

                // register ALU operations: dest ← OP(f,W)
                // 0b_0ooo_ooda 
                case 0b_0000_0010: // no MULWF with d = 0
                case 0b_0000_0011: p.MULWF(loByte, Access(hiByte)); return 2;
                case 0b_0000_0100:
                case 0b_0000_0101:
                case 0b_0000_0110:
                case 0b_0000_0111: p.DECF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_0000:
                case 0b_0001_0001:
                case 0b_0001_0010:
                case 0b_0001_0011: p.IORWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_0100:
                case 0b_0001_0101:
                case 0b_0001_0110:
                case 0b_0001_0111: p.ANDWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_1000:
                case 0b_0001_1001:
                case 0b_0001_1010:
                case 0b_0001_1011: p.XORWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0001_1100:
                case 0b_0001_1101:
                case 0b_0001_1110:
                case 0b_0001_1111: p.COMF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_0000:
                case 0b_0010_0001:
                case 0b_0010_0010:
                case 0b_0010_0011: p.ADDWFC(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_0100:
                case 0b_0010_0101:
                case 0b_0010_0110:
                case 0b_0010_0111: p.ADDWF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_1000:
                case 0b_0010_1001:
                case 0b_0010_1010:
                case 0b_0010_1011: p.INCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0010_1100:
                case 0b_0010_1101:
                case 0b_0010_1110:
                case 0b_0010_1111: p.DECFSZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_0000:
                case 0b_0011_0001:
                case 0b_0011_0010:
                case 0b_0011_0011: p.RRCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_0100:
                case 0b_0011_0101:
                case 0b_0011_0110:
                case 0b_0011_0111: p.RLCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_1000:
                case 0b_0011_1001:
                case 0b_0011_1010:
                case 0b_0011_1011: p.SWAPF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0011_1100:
                case 0b_0011_1101:
                case 0b_0011_1110:
                case 0b_0011_1111: p.INCFSZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_0000:
                case 0b_0100_0001:
                case 0b_0100_0010:
                case 0b_0100_0011: p.RRNCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_0100:
                case 0b_0100_0101:
                case 0b_0100_0110:
                case 0b_0100_0111: p.RLNCF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_1000:
                case 0b_0100_1001:
                case 0b_0100_1010:
                case 0b_0100_1011: p.INFSNZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0100_1100:
                case 0b_0100_1101:
                case 0b_0100_1110:
                case 0b_0100_1111: p.DCFSNZ(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_0000:
                case 0b_0101_0001:
                case 0b_0101_0010:
                case 0b_0101_0011: p.MOVF(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_0100:
                case 0b_0101_0101:
                case 0b_0101_0110:
                case 0b_0101_0111: p.SUBFWB(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_1000:
                case 0b_0101_1001:
                case 0b_0101_1010:
                case 0b_0101_1011: p.SUBWFB(loByte, Destination(hiByte), Access(hiByte)); return 2;
                case 0b_0101_1100:
                case 0b_0101_1101:
                case 0b_0101_1110:
                case 0b_0101_1111: p.SUBWF(loByte, Destination(hiByte), Access(hiByte)); return 2;

                // register ALU operations, do not write to W
                // 0b_0110_oooa f
                case 0b_0110_0000:
                case 0b_0110_0001: p.CPFSLT(loByte, Access(hiByte)); return 2;
                case 0b_0110_0010:
                case 0b_0110_0011: p.CPFSEQ(loByte, Access(hiByte)); return 2;
                case 0b_0110_0100:
                case 0b_0110_0101: p.CPFSGT(loByte, Access(hiByte)); return 2;
                case 0b_0110_0110:
                case 0b_0110_0111: p.TSTFSZ(loByte, Access(hiByte)); return 2;
                case 0b_0110_1000:
                case 0b_0110_1001: p.SETF(loByte, Access(hiByte)); return 2;
                case 0b_0110_1010:
                case 0b_0110_1011: p.CLRF(loByte, Access(hiByte)); return 2;
                //case 0b_0110_1100:
                //case 0b_0110_1101: p.NEGF(loByte, Access(hiByte)); return 2;
                case 0b_0110_1110:
                case 0b_0110_1111: p.MOVWF(loByte, Access(hiByte)); return 2;

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
                case 0b_0111_1111: p.BTG(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;

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
                case 0b_1000_1111: p.BSF(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1001_1111: p.BCF(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1010_1111: p.BTFSS(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;
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
                case 0b_1011_1111: p.BTFSC(loByte, BitOpBit(hiByte), Access(hiByte)); return 2;

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
                        if (data.Length < offset + 4)
                            return 0;
                        byte exHi = data[offset + 3];
                        byte exLo = data[offset + 2];

                        p.MOVFF(MovffSource(hiByte, loByte), MovffDest(exHi, exLo));
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
                case 0b_1101_0111: p.BRA(BraRCallOffset(hiByte, loByte)); return 2;

                // 0b_1101_1nnn
                case 0b_1101_1000:
                case 0b_1101_1001:
                case 0b_1101_1010:
                case 0b_1101_1011:
                case 0b_1101_1100:
                case 0b_1101_1101:
                case 0b_1101_1110:
                case 0b_1101_1111: p.RCALL(BraRCallOffset(hiByte, loByte)); return 2;

                // Conditional branch (to PC+2n)
                // 0b_1110_0ccc n 
                case 0b_1110_0000: p.BZ(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0001: p.BNZ(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0010: p.BC(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0011: p.BNC(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0100: p.BOV(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0101: p.BNOV(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0110: p.BN(ConditionalOffset(loByte)); return 2;
                case 0b_1110_0111: p.BNN(ConditionalOffset(loByte)); return 2;

                // CALL k
                case 0b_1110_1100:
                case 0b_1110_1101:
                    {
                        if (data.Length < offset + 4)
                            return 0;
                        byte exHi = data[offset + 3];
                        byte exLo = data[offset + 2];
                        if ((exHi & 0xf0) != 0xf0)
                            goto unknown;
                        p.CALL(CallGotoAddr(loByte, exHi, exLo), CallReturnMode(hiByte));
                        return 4;
                    }

                // LFSR n
                case 0b_1110_1110:
                    {
                        if ((loByte & 0xC0) != 0)
                            goto unknown;
                        if (data.Length < offset + 4)
                            return 0;
                        byte exHi = data[offset + 3];
                        byte exLo = data[offset + 2];
                        if (exHi  != 0xf0)
                            goto unknown;
                        p.LFSR((loByte >> 4) & 3, exLo | ((loByte & 0xf) << 8));
                        return 4;
                    }

                // GOTO k
                case 0b_1110_1111:
                    {
                        if (data.Length < offset + 4)
                            return 0;
                        byte exHi = data[offset + 3];
                        byte exLo = data[offset + 2];
                        if ((exHi & 0xf0) != 0xf0)
                            goto unknown;
                        p.GOTO(CallGotoAddr(loByte, exHi, exLo));
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
                case 0b_1111_1111: p.NOPEX(hiByte, loByte); return 2;

                default: goto unknown;
            }

            unknown:
            p.Unknown(hiByte, loByte);
            return 2;
        }
    }
}
