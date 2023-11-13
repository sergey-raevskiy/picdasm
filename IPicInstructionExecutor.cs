namespace picdasm
{
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
}
