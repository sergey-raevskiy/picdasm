using System;
using System.Collections.Generic;

namespace picdasm
{
    interface IPicInstructionExecutorNoDispatch
    {
        void Exec(int pc, PicInstructionBuf buf, PicInstrucitonType type);
    }

    class XorSwitchMetaInstructionProcessor : IPicInstructionExecutorNoDispatch
    {
        public XorSwitchMetaInstructionProcessor(Writer o)
        {
            this.o = o;
        }

        enum State
        {
            WaitXor,
            WaitCheckZ,
            WaitGoto,
        }

        private class XorSwitchSeq
        {
            public byte literal;
            public int jumpAddr;
        }

        State state;

        int pc;
        private int startPc;
        private int endPc;
        private List<XorSwitchSeq> seq = new List<XorSwitchSeq>();
        private readonly Writer o;

        public void Exec(int pc, PicInstructionBuf buf, PicInstrucitonType type)
        {
            this.pc = pc;
            if (type == PicInstrucitonType.XORLW)
            {
                XORLW(buf.Literal);
            }
            else if (type == PicInstrucitonType.BZ)
            {
                BZ(buf.ConditionalOffset);
            }
            else if (type == PicInstrucitonType.BTFSC)
            {
                BTFSC(buf.FileReg, buf.BitOpBit, buf.Access);
            }
            else if (type == PicInstrucitonType.BRA)
            {
                BRA(buf.BraRCallOffset);
            }
            else
            {
                ResetState();
            }
        }

        protected void ResetState()
        {
            if (state == State.WaitXor && seq.Count > 0)
            {
                List<string> lines = new List<string>();

                byte st = 0;
                lines.Add("switch (W)");
                lines.Add("{");
                for (int i =0; i < seq.Count;)
                {
                    var s = seq[i];

                    if (i>0)
                        lines.Add(null);

                    while (i < seq.Count && seq[i].jumpAddr == s.jumpAddr)
                    {
                        st ^= seq[i].literal;
                        lines.Add(string.Format("case 0x{0:X2}:", st));
                        i++;
                    }

                    lines[lines.Count - 1] += string.Format(" goto _0x{0:X5};", s.jumpAddr);
                }
                lines.Add("}");
                lines.Add("/* default: */");

                o.Rewrite(startPc, endPc, lines.ToArray());
                o.RefGoto(startPc);
                o.RefGoto(endPc);

            }

            seq.Clear();
            state = State.WaitXor;
        }

        public void XORLW(byte literal)
        {
            if (state == State.WaitXor)
            {
                if (seq.Count == 0)
                    startPc = pc;
                seq.Add(new XorSwitchSeq() { literal = literal });
                state = State.WaitCheckZ;
            }
            else
            {
                ResetState();
            }
        }

        public void BZ(int off)
        {
            if (state == State.WaitCheckZ)
            {
                int addr = pc + 2 + 2 * off;
                seq[seq.Count - 1].jumpAddr = addr;
                state = State.WaitXor;
                endPc = pc + 2;
            }
            else
            {
                ResetState();
            }
        }

        public void BTFSC(byte addr, int bit, AccessMode access)
        {
            if (state == State.WaitCheckZ && IsZeroBit(addr, bit, access))
            {
                state = State.WaitGoto;
            }
            else
            {
                ResetState();
            }
        }

        public void BRA(int off)
        {
            if (state == State.WaitGoto)
            {
                int addr = pc + 2 + 2 * off;
                seq[seq.Count - 1].jumpAddr = addr;
                state = State.WaitXor;
                endPc = pc + 2;
            }
            else
            {
                ResetState();
            }
        }

        private static bool IsZeroBit(byte addr, int bit, AccessMode access)
        {
            if (access == AccessMode.Access)
            {
                int absAddr = (addr - 96 + 0x0f60) & 0xfff;
                return absAddr == 0xFD8 && bit == 2;
            }

            return false;
        }
    }
}
