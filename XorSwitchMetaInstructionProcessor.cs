using System;
using System.Collections.Generic;

namespace picdasm
{
    interface IPicInstructionExecutorNoDispatch
    {
        void Exec(int pc, PicInstructionBuf buf, PicInstrucitonType type);
    }

    abstract class MetaInstructionProcessorBase2 : IPicInstructionExecutorNoDispatch
    {
        protected class CurrentInstructionHandle
        {
            public int PC;
            public PicInstructionBuf Buf;
            public PicInstrucitonType InstructionType;
        }

        protected enum NextAction
        {
            Next,
            Reset,
            Completed,
        }

        CurrentInstructionHandle h = new CurrentInstructionHandle();
        IEnumerator<NextAction> iter;

        protected MetaInstructionProcessorBase2()
        {
        }

        protected abstract IEnumerable<NextAction> QQ(CurrentInstructionHandle h);

        public void Exec(int pc, PicInstructionBuf buf, PicInstrucitonType type)
        {
            h.PC = pc;
            h.Buf = buf;
            h.InstructionType = type;

            if (iter == null)
            {
                iter = QQ(h).GetEnumerator();
            }

            if (!iter.MoveNext() || iter.Current == NextAction.Reset || iter.Current == NextAction.Completed)
            {
                iter = null;
            }
        }
    }

    class XorSwitchMetaInstructionProcessor : MetaInstructionProcessorBase2
    {
        public XorSwitchMetaInstructionProcessor(Writer o)
        {
            this.o = o;
        }

        private class XorSwitchSeq
        {
            public byte literal;
            public int jumpAddr;
        }

        private readonly Writer o;
        
        private void DumpSeq(List<XorSwitchSeq> seq, int startPc, int endPc)
        {
            List<string> lines = new List<string>();

            byte st = 0;
            lines.Add("switch (W)");
            lines.Add("{");
            for (int i = 0; i < seq.Count;)
            {
                var s = seq[i];

                if (i > 0)
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

        override protected IEnumerable<NextAction> QQ(CurrentInstructionHandle h)
        {
            int startPc = 0;
            int endPc = 0;
            List<XorSwitchSeq> seq = new List<XorSwitchSeq>();

            while (true)
            {
                if (h.InstructionType == PicInstrucitonType.XORLW)
                {
                    if (seq.Count == 0)
                        startPc = h.PC;
                    seq.Add(new XorSwitchSeq() { literal = h.Buf.Literal });
                    yield return NextAction.Next;
                }
                else
                {
                    if (seq.Count > 0)
                    {
                        DumpSeq(seq, startPc, endPc);
                    }

                    yield return NextAction.Reset;
                }

                if (h.InstructionType == PicInstrucitonType.BZ)
                {
                    int addr = h.PC + 2 + 2 * h.Buf.ConditionalOffset;
                    seq[seq.Count - 1].jumpAddr = addr;
                    endPc = h.PC + 2;
                    yield return NextAction.Next;
                }
                else if (h.InstructionType == PicInstrucitonType.BTFSC &&
                         IsZeroBit(h.Buf.FileReg, h.Buf.BitOpBit, h.Buf.Access))
                {
                    yield return NextAction.Next;

                    if (h.InstructionType == PicInstrucitonType.BRA)
                    {
                        int addr = h.PC + 2 + 2 * h.Buf.BraRCallOffset;
                        seq[seq.Count - 1].jumpAddr = addr;
                        endPc = h.PC + 2;
                        yield return NextAction.Next;
                    }
                    else
                    {
                        yield return NextAction.Reset;
                    }
                }
                else
                {
                    yield return NextAction.Reset;
                }
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
