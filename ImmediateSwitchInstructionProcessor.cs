using System.Collections.Generic;

namespace picdasm
{
    class ImmediateSwitchInstructionProcessor : MetaInstructionProcessorBase
    {
        private Writer o;
        private int validCases;
        private int[] casesJumpAddrs;
        byte baseH, baseU, baseL;
        int casen;
        int startPc, endPc;
        bool isshort;
        State state;

        enum State
        {
            Wait_SUBLF_ValidCases,
            Wait_BTFSS_Carry,
            Wait_GOTO_Default,
            Wait_MOVLW_BaseH,
            Wait_MOVWF_PCLATH,
            Wait_MOVLW_BaseU,
            Wait_MOVWF_PCLATU,
            Wait_RLNCF,
            Wait_ADDLW_BaseL,
            Wait_BTFSC_Carry,
            Wait_INCF_PCLATH,
            Wait_MOVWF_PCL,
            Wait_BRA_CaseN,
        }

        protected override void ResetState()
        {
            if (state == State.Wait_BRA_CaseN && casen == validCases)
            {
                Rewrite();
            }

            state = State.Wait_SUBLF_ValidCases;
            base.ResetState();
        }

        public ImmediateSwitchInstructionProcessor(Writer o)
        {
            this.o = o;
        }

        public override void SUBLW(byte literal)
        {
            validCases = literal + 1;
            casesJumpAddrs = new int[validCases + 1];
            startPc = pc;
            state = State.Wait_BTFSS_Carry;
        }

        public override void BTFSS(byte addr, int bit, AccessMode access)
        {
            if (state == State.Wait_BTFSS_Carry && 
                access == AccessMode.Access && 
                addr == 0xd8 && 
                bit == 0)
            {
                state = State.Wait_GOTO_Default;
            }
            else
            {
                ResetState();
            }
        }

        public override void GOTO(int addr)
        {
            if (state == State.Wait_GOTO_Default)
            {
                casesJumpAddrs[casesJumpAddrs.Length-1] = addr * 2;
                state = State.Wait_MOVLW_BaseH;
            }
            else
            {
                ResetState();
            }
        }

        public override void MOVLW(byte literal)
        {
            if (state == State.Wait_MOVLW_BaseH)
            {
                baseH = literal;
                state = State.Wait_MOVWF_PCLATH;
            }
            else if (state == State.Wait_MOVLW_BaseU)
            {
                baseU = literal;
                state = State.Wait_MOVWF_PCLATU;
            }
            else
            {
                ResetState();
            }
        }

        public override void MOVWF(byte addr, AccessMode access)
        {
            if (state == State.Wait_MOVWF_PCLATH &&
                access == AccessMode.Access &&
                addr == 0xFA)
            {
                state = State.Wait_MOVLW_BaseU;
            }
            else if (state == State.Wait_MOVWF_PCLATU &&
                     access == AccessMode.Access &&
                     addr == 0xFB)
            {
                state = State.Wait_RLNCF;
            }
            else if (state == State.Wait_MOVWF_PCL &&
                     access == AccessMode.Access &&
                     addr == 0xF9)
            {
                int baseAddr = (baseU << 16) | (baseH << 8) | baseL;
                if (baseAddr == pc+2)
                {
                    state = State.Wait_BRA_CaseN;
                    casen = 0;
                }
                else
                {
                    ResetState();
                }
            }
            else
            {
                ResetState();
            }
        }

        public override void RLNCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (state == State.Wait_RLNCF)
            {
                state = State.Wait_ADDLW_BaseL;
                isshort = false;
            }
            else if (state == State.Wait_MOVLW_BaseU)
            {
                state = State.Wait_ADDLW_BaseL;
                isshort = true;
                baseU = 0;
            }
            else
            {
                ResetState();
            }
        }

        public override void ADDLW(byte literal)
        {
            if (state == State.Wait_ADDLW_BaseL)
            {
                baseL = literal;
                state = State.Wait_BTFSC_Carry;
            }
            else
            {
                ResetState();
            }
        }

        public override void BTFSC(byte addr, int bit, AccessMode access)
        {
            if (state == State.Wait_BTFSC_Carry &&
                access == AccessMode.Access &&
                addr == 0xd8 &&
                bit == 0)
            {
                state = State.Wait_INCF_PCLATH;
            }
            else
            {
                ResetState();
            }
        }

        public override void INCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (state == State.Wait_INCF_PCLATH &&
                access == AccessMode.Access &&
                addr == 0xFA &&
                dest == DestinationMode.F)
            {
                state = State.Wait_MOVWF_PCL;
            }
            else
            {
                ResetState();
            }
        }

        public override void BRA(int off)
        {
            if (state == State.Wait_BRA_CaseN && casen < validCases + 1)
            {
                int addr = pc + 2 + 2 * off;
                casesJumpAddrs[casen] = addr;
                casen++;
                endPc = pc;
            }
            else
            {
                ResetState();
            }
        }

        private void Rewrite()
        {
            List<string> lines = new List<string>();

            if (isshort)
            {
                lines.Add("/* short */");
            }

            lines.Add("switch (W)");
            lines.Add("{");
            for (int i = 0; i < casesJumpAddrs.Length;)
            {
                if (i > 0)
                    lines.Add(null);

                int addr = casesJumpAddrs[i];
                while (i < casesJumpAddrs.Length && casesJumpAddrs[i] == addr)
                {
                    if (i == casesJumpAddrs.Length - 1)
                        lines.Add("default:");
                    else
                        lines.Add(string.Format("case 0x{0:X2}:", i));
                    i++;
                }

                lines[lines.Count - 1] += string.Format(" goto _0x{0:X5};", addr);
            }
            lines.Add("}");

            o.Rewrite(startPc, endPc, lines.ToArray());
            o.RefGoto(startPc);
            o.RefGoto(endPc);
        }
    }
}
