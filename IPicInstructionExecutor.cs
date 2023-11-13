using System;

namespace picdasm
{
    class Driver : IPicInstructionExecutorNoDispatch
    {
        private readonly IPicInstructionExecutor e;

        public Driver(IPicInstructionExecutor e)
        {
            this.e = e;
        }

        public void Exec(int pc, PicInstructionBuf buf, PicInstrucitonType type)
        {
            e.SetPc(pc);

            switch (type)
            {
                case PicInstrucitonType.NOP: e.NOP(); return;
                case PicInstrucitonType.SLEEP: e.SLEEP(); return;
                case PicInstrucitonType.CLRWDT: e.CLRWDT(); return;
                case PicInstrucitonType.PUSH: e.PUSH(); return;
                case PicInstrucitonType.POP: e.POP(); return;
                case PicInstrucitonType.DAW: e.DAW(); return;

                case PicInstrucitonType.TBLRD: e.TBLRD(buf.TableMode); return;
                case PicInstrucitonType.TBLWT: e.TBLWT(buf.TableMode); return;

                case PicInstrucitonType.RETFIE: e.RETFIE(buf.ReturnRetfieMode); return;
                case PicInstrucitonType.RETURN: e.RETURN(buf.ReturnRetfieMode); return;

                case PicInstrucitonType.CALLW: e.CALLW(); return;

                case PicInstrucitonType.EMULEN: e.EMULEN(); return;
                case PicInstrucitonType.EMULDIS: e.EMULDIS(); return;

                case PicInstrucitonType.RESET: e.RESET(); return;

                case PicInstrucitonType.MOVLB: e.MOVLB(buf.Bank); return;

                case PicInstrucitonType.SUBLW: e.SUBLW(buf.Literal); return;
                case PicInstrucitonType.IORLW: e.IORLW(buf.Literal); return;
                case PicInstrucitonType.XORLW: e.XORLW(buf.Literal); return;
                case PicInstrucitonType.ANDLW: e.ANDLW(buf.Literal); return;
                case PicInstrucitonType.RETLW: e.RETLW(buf.Literal); return;
                case PicInstrucitonType.MULLW: e.MULLW(buf.Literal); return;
                case PicInstrucitonType.MOVLW: e.MOVLW(buf.Literal); return;
                case PicInstrucitonType.ADDLW: e.ADDLW(buf.Literal); return;

                // register ALU operations: e.dest ← OP(f,W)
                // 0b_0ooo_ooda 
                case PicInstrucitonType.MULWF: e.MULWF(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.DECF: e.DECF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.IORWF: e.IORWF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.ANDWF: e.ANDWF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.XORWF: e.XORWF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.COMF: e.COMF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.ADDWFC: e.ADDWFC(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.ADDWF: e.ADDWF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.INCF: e.INCF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.DECFSZ: e.DECFSZ(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.RRCF: e.RRCF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.RLCF: e.RLCF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.SWAPF: e.SWAPF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.INCFSZ: e.INCFSZ(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.RRNCF: e.RRNCF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.RLNCF: e.RLNCF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.INFSNZ: e.INFSNZ(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.DCFSNZ: e.DCFSNZ(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.MOVF: e.MOVF(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.SUBFWB: e.SUBFWB(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.SUBWFB: e.SUBWFB(buf.FileReg, buf.Destination, buf.Access); return;
                case PicInstrucitonType.SUBWF: e.SUBWF(buf.FileReg, buf.Destination, buf.Access); return;

                // register ALU operations, do not write to W
                // 0b_0110_oooa f
                case PicInstrucitonType.CPFSLT: e.CPFSLT(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.CPFSEQ: e.CPFSEQ(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.CPFSGT: e.CPFSGT(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.TSTFSZ: e.TSTFSZ(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.SETF: e.SETF(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.CLRF: e.CLRF(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.NEGF: e.NEGF(buf.FileReg, buf.Access); return;
                case PicInstrucitonType.MOVWF: e.MOVWF(buf.FileReg, buf.Access); return;

                // 0b_0111_bbba BTG f,b,a Toggle bit b of f
                case PicInstrucitonType.BTG: e.BTG(buf.FileReg, buf.BitOpBit, buf.Access); return;

                // register Bit operations
                // 0b_10oo_bbba 
                case PicInstrucitonType.BSF: e.BSF(buf.FileReg, buf.BitOpBit, buf.Access); return;
                case PicInstrucitonType.BCF: e.BCF(buf.FileReg, buf.BitOpBit, buf.Access); return;
                case PicInstrucitonType.BTFSS: e.BTFSS(buf.FileReg, buf.BitOpBit, buf.Access); return;
                case PicInstrucitonType.BTFSC: e.BTFSC(buf.FileReg, buf.BitOpBit, buf.Access); return;

                // MOVFF
                case PicInstrucitonType.MOVFF: e.MOVFF(buf.MovffSource, buf.MovffDest); return;

                // BRA n
                case PicInstrucitonType.BRA: e.BRA(buf.BraRCallOffset); return;
                case PicInstrucitonType.RCALL: e.RCALL(buf.BraRCallOffset); return;

                // Conditional branch (to PC+2n)
                case PicInstrucitonType.BZ: e.BZ(buf.ConditionalOffset); return;
                case PicInstrucitonType.BNZ: e.BNZ(buf.ConditionalOffset); return;
                case PicInstrucitonType.BC: e.BC(buf.ConditionalOffset); return;
                case PicInstrucitonType.BNC: e.BNC(buf.ConditionalOffset); return;
                case PicInstrucitonType.BOV: e.BOV(buf.ConditionalOffset); return;
                case PicInstrucitonType.BNOV: e.BNOV(buf.ConditionalOffset); return;
                case PicInstrucitonType.BN: e.BN(buf.ConditionalOffset); return;
                case PicInstrucitonType.BNN: e.BNN(buf.ConditionalOffset); return;

                case PicInstrucitonType.ADDFSR: e.ADDFSR(buf.FsrN, buf.FsrLiteral); return;
                case PicInstrucitonType.ADDULNK: e.ADDULNK(buf.FsrLiteral); return;
                case PicInstrucitonType.SUBFSR: e.SUBFSR(buf.FsrN, buf.FsrLiteral); return;
                case PicInstrucitonType.SUBULNK: e.SUBULNK(buf.FsrLiteral); return;

                case PicInstrucitonType.PUSHL: e.PUSHL(buf.Literal); return;

                case PicInstrucitonType.MOVSF: e.MOVSF(buf.MovssSrc, buf.MovsfFileReg); return;
                case PicInstrucitonType.MOVSS: e.MOVSS(buf.MovssSrc, buf.MovssDst); return;

                case PicInstrucitonType.LFSR: e.LFSR(buf.LfsrFSR, buf.LfsrLiteral); return;

                case PicInstrucitonType.CALL: e.CALL(buf.CallGotoAddr, buf.CallMode); return;
                case PicInstrucitonType.GOTO: e.GOTO(buf.CallGotoAddr); return;

                case PicInstrucitonType.NOPEX: e.NOPEX(buf.NopExHi, buf.NopExLo); return;

                case PicInstrucitonType.Unknown: e.Unknown(buf.UnkHi, buf.UnkLo); return;

                default:
                    throw new NotImplementedException(string.Format("Unknown type ({0})", type));
            }
        }
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

        // Literal operations: e.W ← OP(k,W)
        void SUBLW(byte literal);
        void IORLW(byte literal);
        void XORLW(byte literal);
        void ANDLW(byte literal);
        void RETLW(byte literal);
        void MULLW(byte literal);
        void MOVLW(byte literal);
        void ADDLW(byte literal);

        // register ALU operations: e.dest ← OP(f,W)
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
