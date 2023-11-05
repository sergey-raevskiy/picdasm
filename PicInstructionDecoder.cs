using System;

namespace picdasm
{
    class PicInstructionDecoder
    {
        private InstructionDecoderHelper helper;

        public PicInstructionDecoder(byte[] data)
        {
            helper = new InstructionDecoderHelper(data);
        }

        private bool match;
        private PicInstructionDecoder Pattern(string pattern)
        {
            match = helper.Match(pattern);
            return this;
        }

        private PicInstructionDecoder Bind(char field, out byte val)
        {
            if (match)
            {
                val = (byte)helper.Fields[field];
            }
            else
            {
                val = 0;
            }

            return this;
        }

        private PicInstructionDecoder Bind(char field, out int val)
        {
            if (match)
            {
                val = (int)helper.Fields[field];
            }
            else
            {
                val = 0;
            }

            return this;
        }

        private PicInstructionDecoder Bind<T>(char field, out T val)
            where T : Enum
        {
            if (match)
            {
                val = (T)(object)(int)helper.Fields[field];
            }
            else
            {
                val = default(T);
            }

            return this;
        }

        private bool Match()
        {
            return match;
        }

        public bool IsMovLW(out byte literal)
        {
            return
                Pattern("0000 1110 kkkk kkkk")
                .Bind('k', out literal)
                .Match();
        }

        public bool IsMovWF(out byte addr, out AddrMode mode)
        {
            return
                Pattern("0110 111a ffff ffff")
                .Bind('f', out addr)
                .Bind('a', out mode)
                .Match();
        }

        public bool IsBra(out int offset)
        {
            return
                Pattern("1101 0`nnn nnnn nnnn")
                .Bind('n', out offset)
                .Match();
        }
    }
}
