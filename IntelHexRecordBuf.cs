namespace picdasm
{
    enum IntelHexRecordType
    {
        Data = 0x00,
        EndOfFile = 0x01,
        ExtendedLinearAddress = 0x04,
    }

    class IntelHexRecordBuf
    {
        public int Length;
        public int Address;
        public IntelHexRecordType RecordType;
        public readonly byte[] DataBuf;
        public byte CheckSum;
        public bool CheckSumValid;

        public IntelHexRecordBuf()
        {
            DataBuf = new byte[IntelHexRecordParser.MaxDataBufLen];
        }
    }
}
