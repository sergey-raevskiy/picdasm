using System;

namespace picdasm
{
    enum PicInstructionKind
    {
        Unknown,
        Literal,
    }

    enum PicLiteralInstruction : byte
    {
        // 0 0 0 0  1 opcode:3 literal:8

        Prefix = 0x08,
        PrefixMask = 0xF8,
        OpCodeMask = 0x07,

        SUBLW = 0x0,
        IORLW = 0x1,
        XORLW = 0x2,
        ANDLW = 0x3,
        RETLW = 0x4,
        MULLW = 0x5,
        MOVLW = 0x6,
        ADDLW = 0x7,
    }

    class PicInstructionBuf
    {
        public int InstructionLength;
        public PicInstructionKind InstrucitonKind;

        // PicInstructionKind.Literal
        public PicLiteralInstruction LiteralInstruction;
        public byte LiteralInstuctionLiteral;
    }

    class PicInstructionDecoder
    {
        private readonly byte[] data;

        public PicInstructionDecoder(byte[] data)
        {
            this.data = data;
        }

        public bool DecodeAt(PicInstructionBuf buf, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return false;
            }

            byte ir = data[offset + 1];
            if ((byte)(ir & (byte)PicLiteralInstruction.PrefixMask) == (byte) PicLiteralInstruction.Prefix)
            {
                buf.InstrucitonKind = PicInstructionKind.Literal;
                buf.LiteralInstruction = (PicLiteralInstruction)(ir & (byte)PicLiteralInstruction.OpCodeMask);
                buf.LiteralInstuctionLiteral = data[offset];

                buf.InstructionLength = 2;
                return true;
            }

            buf.InstrucitonKind = PicInstructionKind.Unknown;
            buf.InstructionLength = 2;
            return true;
        }
    }
}
