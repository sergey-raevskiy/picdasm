namespace picdasm
{
    class Context
    {
        private byte[] progmem;
        public int PC { get; set; }

        public Context(byte[] progmem)
        {
            this.progmem = progmem;
        }

        public void Fetch(PicInstructionBuf buf)
        {
            buf.hiByte = progmem[PC + 1];
            buf.loByte = progmem[PC];
            buf.isLong = false;
        }

        public void FetchLong(PicInstructionBuf buf)
        {
            buf.hiByte = progmem[PC + 1];
            buf.loByte = progmem[PC];
            buf.exHi = progmem[PC + 3];
            buf.exLo = progmem[PC + 2];
            buf.isLong = true;
        }

        public ushort ReadU16(int addr)
        {
            byte hiByte = progmem[addr + 1];
            byte loByte = progmem[addr];
            return (ushort)((hiByte << 8) | loByte);
        }
    }
}
