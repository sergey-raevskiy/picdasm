using System;
using System.Diagnostics;

namespace picdasm
{
    class InstructionDecoderHelper
    {
        private readonly byte[] data;
        private int pos;

        public int Pos()
        {
            return pos;
        }

        public uint[] Fields = new uint[Math.Max('z', 'Z') + 1];

        public InstructionDecoderHelper(byte[] data)
        {
            this.data = data;
            this.pos = 0;
        }

        public bool Eof()
        {
            return pos >= data.Length;
        }

        public bool Match(string pattern)
        {
            int pos = this.pos;
            if (pos >= data.Length)
                return false;

            int mask = 0x80;
            ulong fields = 0;
            bool signextend = false;

            for (int i = 0; i < pattern.Length; i++)
            {
                char p = pattern[i];

                if (p == ' ')
                    continue;
                else if (p == '`')
                {
                    signextend = true;
                    continue;
                }

                if (mask == 0)
                {
                    pos++;
                    if (pos >= data.Length)
                        return false;

                    mask = 0x80;
                }

                uint bit = (data[pos] & mask) != 0 ? 1u : 0u;

                if (p == '0')
                {
                    if (bit != 0)
                        return false;

                    Debug.Assert(!signextend);
                }
                else if (p == '1')
                {
                    if (bit != 1)
                        return false;

                    Debug.Assert(!signextend);
                }
                else if ((p >= 'a' && p <= 'z') || (p >= 'A' && p <= 'Z'))
                {
                    // hack
                    ulong fieldMask = (1u << (p - 'A'));

                    if ((fields & fieldMask) == 0)
                    {
                        if (signextend && bit == 1)
                            Fields[p] = 0xffffffff;
                        else
                            Fields[p] = 0;

                        signextend = false;
                        fields |= fieldMask;
                    }

                    Debug.Assert(!signextend);
                    Fields[p] <<= 1;
                    Fields[p] |= bit;
                }

                mask >>= 1;
                signextend = false;
            }

            if (mask != 0)
                throw new Exception(string.Format("Invalid pattern ({0})", pattern));

            this.pos = pos + 1;
            return true;
        }

        public bool Match(params object[] pattern)
        {
            return true;
        }
    }
}
