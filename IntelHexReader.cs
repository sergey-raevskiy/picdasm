using System;
using System.IO;

namespace picdasm
{
    class IntelHexReader
    {
        public const int MaxDataBufLen = IntelHexRecordParser.MaxDataBufLen;

        private readonly TextReader textReader;

        public IntelHexReader(TextReader textReader)
        {
            this.textReader = textReader;
        }

        public void ReadRecord(IntelHexRecordBuf buf)
        {
            string rec = textReader.ReadLine();

            if (rec == null)
                throw new EndOfStreamException("Unexpected end of file reached");

            if (rec.Length < 1 || rec[0] != ':')
                throw new Exception(string.Format("Invalid record ({0})", rec));

            IntelHexRecordParser.ParseRecord(rec, buf);
        }
    }
}
