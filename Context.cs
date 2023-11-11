namespace picdasm
{
    class Context : IPicInstructionFetcher
    {
        private byte[] progmem;
        public int PC { get; set; }

        public Context(byte[] progmem)
        {
            this.progmem = progmem;
        }

        public void FetchInstruciton(out byte hi, out byte lo)
        {
            hi = progmem[PC + 1];
            lo = progmem[PC];
            PC += 2;
        }
    }
}
