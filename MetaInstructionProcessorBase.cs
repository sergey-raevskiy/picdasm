namespace picdasm
{
    class MetaInstructionProcessorBase : IPicInstructionExecutor
    {
        public MetaInstructionProcessorBase() 
        {
            ResetState();
        }

        protected int pc;

        virtual public void SetPc(int pc)
        {
            this.pc = pc;
        }

        protected virtual void ResetState()
        {
        }

        virtual public void ADDFSR(int n, int k)
        {
            ResetState();
        }

        virtual public void ADDLW(byte literal)
        {
            ResetState();
        }

        virtual public void ADDULNK(int k)
        {
            ResetState();
        }

        virtual public void ADDWF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void ADDWFC(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void ANDLW(byte literal)
        {
            ResetState();
        }

        virtual public void ANDWF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void BC(int off)
        {
            ResetState();
        }

        virtual public void BCF(byte addr, int bit, AccessMode access)
        {
            ResetState();
        }

        virtual public void BN(int off)
        {
            ResetState();
        }

        virtual public void BNC(int off)
        {
            ResetState();
        }

        virtual public void BNN(int off)
        {
            ResetState();
        }

        virtual public void BNOV(int off)
        {
            ResetState();
        }

        virtual public void BNZ(int off)
        {
            ResetState();
        }

        virtual public void BOV(int off)
        {
            ResetState();
        }

        virtual public void BRA(int offset)
        {
            ResetState();
        }

        virtual public void BSF(byte addr, int bit, AccessMode access)
        {
            ResetState();
        }

        virtual public void BTFSC(byte addr, int bit, AccessMode access)
        {
            ResetState();
        }

        virtual public void BTFSS(byte addr, int bit, AccessMode access)
        {
            ResetState();
        }

        virtual public void BTG(byte addr, int bit, AccessMode access)
        {
            ResetState();
        }

        virtual public void BZ(int off)
        {
            ResetState();
        }

        virtual public void CALL(int addr, CallReturnOpMode mode)
        {
            ResetState();
        }

        virtual public void CALLW()
        {
            ResetState();
        }

        virtual public void CLRF(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void CLRWDT()
        {
            ResetState();
        }

        virtual public void COMF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void CPFSEQ(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void CPFSGT(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void CPFSLT(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void DAW()
        {
            ResetState();
        }

        virtual public void DCFSNZ(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void DECF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void DECFSZ(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void EMULDIS()
        {
            ResetState();
        }

        virtual public void EMULEN()
        {
            ResetState();
        }

        virtual public void GOTO(int addr)
        {
            ResetState();
        }

        virtual public void INCF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void INCFSZ(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void INFSNZ(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void IORLW(byte literal)
        {
            ResetState();
        }

        virtual public void IORWF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void LFSR(int f, int k)
        {
            ResetState();
        }

        virtual public void MOVF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void MOVFF(int source, int dest)
        {
            ResetState();
        }

        virtual public void MOVLB(int literal)
        {
            ResetState();
        }

        virtual public void MOVLW(byte literal)
        {
            ResetState();
        }

        virtual public void MOVSF(int src, int dst)
        {
            ResetState();
        }

        virtual public void MOVSS(int src, int dst)
        {
            ResetState();
        }

        virtual public void MOVWF(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void MULLW(byte literal)
        {
            ResetState();
        }

        virtual public void MULWF(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void NEGF(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void NOP()
        {
            ResetState();
        }

        virtual public void NOPEX(byte hiByte, byte loByte)
        {
            ResetState();
        }

        virtual public void POP()
        {
            ResetState();
        }

        virtual public void PUSH()
        {
            ResetState();
        }

        virtual public void PUSHL(byte l)
        {
            ResetState();
        }

        virtual public void RCALL(int offset)
        {
            ResetState();
        }

        virtual public void RESET()
        {
            ResetState();
        }

        virtual public void RETFIE(CallReturnOpMode mode)
        {
            ResetState();
        }

        virtual public void RETLW(byte literal)
        {
            ResetState();
        }

        virtual public void RETURN(CallReturnOpMode mode)
        {
            ResetState();
        }

        virtual public void RLCF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void RLNCF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void RRCF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void RRNCF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void SETF(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void SLEEP()
        {
            ResetState();
        }

        virtual public void SUBFSR(int n, int k)
        {
            ResetState();
        }

        virtual public void SUBFWB(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void SUBLW(byte literal)
        {
            ResetState();
        }

        virtual public void SUBULNK(int k)
        {
            ResetState();
        }

        virtual public void SUBWF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void SUBWFB(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void SWAPF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }

        virtual public void TBLRD(TableOpMode mode)
        {
            ResetState();
        }

        virtual public void TBLWT(TableOpMode mode)
        {
            ResetState();
        }

        virtual public void TSTFSZ(byte addr, AccessMode access)
        {
            ResetState();
        }

        virtual public void Unknown(byte hiByte, byte loByte)
        {
            ResetState();
        }

        virtual public void XORLW(byte literal)
        {
            ResetState();
        }

        virtual public void XORWF(byte addr, DestinationMode dest, AccessMode access)
        {
            ResetState();
        }
    }
}
