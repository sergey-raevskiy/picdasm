using System;
using System.Collections.Generic;

namespace picdasm
{
    class XorSwitchMetaInstructionProcessor : MetaInstructionProcessorBase
    {
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

        private int startPc;
        private int endPc;
        private List<XorSwitchSeq> seq = new List<XorSwitchSeq>();

        protected override void ResetState()
        {
            if (state == State.WaitXor && seq.Count > 0)
            {
                byte st = 0;
                Console.WriteLine("_0x{0:X5}:", startPc);
                Console.WriteLine("switch (W)");
                Console.WriteLine("{");
                foreach (XorSwitchSeq s in seq)
                {
                    st ^= s.literal;

                    Console.WriteLine("case 0x{0:X2}: goto _0x{1:X5};", st, s.jumpAddr);
                }
                Console.WriteLine("}");
                Console.WriteLine("/* default: */");
                Console.WriteLine("_0x{0:X5}:", endPc);
                Console.WriteLine("");
            }

            seq.Clear();
            state = State.WaitXor;
            base.ResetState();
        }

        public override void XORLW(byte literal)
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

        public override void BZ(int off)
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

        public override void BTFSC(byte addr, int bit, AccessMode access)
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

        public override void BRA(int off)
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
