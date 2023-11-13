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
            buf.exValid = false;
        }

        public void FetchLong(PicInstructionBuf buf)
        {
            buf.hiByte = progmem[PC + 1];
            buf.loByte = progmem[PC];
            buf.exHi = progmem[PC + 3];
            buf.exLo = progmem[PC + 2];
            buf.exValid = true;
        }
    }
}
