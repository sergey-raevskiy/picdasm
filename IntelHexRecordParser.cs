using System;

namespace picdasm
{
    static class IntelHexRecordParser
    {
        public const int MaxDataBufLen = 256;

        private static int HexVal(char h)
        {
            if (h >= 'A' && h <= 'F')
                return (h - 'A' + 10);
            else if (h >= '0' && h <= '9')
                return (h - '0');
            else
                return -1;
        }

        public static void ParseRecord(string rec,
                                       IntelHexRecordBuf recBuf)
        {
            recBuf.Length = 0;
            recBuf.Address = 0;

            int pos = 1;
            int rem = rec.Length - 1;
            int state = 0;
            int dataPos = 0;
            int checksum = 0;

            while (rem > 1)
            {
                int bh = HexVal(rec[pos++]);
                int bl = HexVal(rec[pos++]);
                rem -= 2;

                int val = (bh >= 0 && bl >= 0) ? (bh * 16 + bl) : -1;

                switch (state)
                {
                    case 0:
                        if (val < 0)
                            throw new Exception(string.Format("Record has an invalid length field ({0})", rec));
                        recBuf.Length =  val;
                        state++;
                        break;
                    case 1:
                        if (val < 0)
                            throw new Exception(string.Format("Record has an invalid address field ({0})", rec));
                        recBuf.Address = val * 256;
                        state++;
                        break;
                    case 2:
                        if (val < 0)
                            throw new Exception(string.Format("Record has an invalid address field ({0})", rec));
                        recBuf.Address += val;
                        state++;
                        break;
                    case 3:
                        if (val < 0)
                            throw new Exception(string.Format("Record has an invalid record type field ({0})", rec));

                        switch (val)
                        {
                            case (int)IntelHexRecordType.Data:
                            case (int)IntelHexRecordType.EndOfFile:
                            case (int)IntelHexRecordType.ExtendedLinearAddress:
                                recBuf.RecordType = (IntelHexRecordType)val;
                                break;

                            default:
                                throw new Exception(string.Format("Record has an unknown record type ({0})", rec));
                        }
                        state++;
                        break;

                    case 4:
                        if (dataPos < recBuf.Length)
                        {
                            if (val < 0)
                                throw new Exception(string.Format("Record has an invalid data byte ({0})", rec));
                            recBuf.DataBuf[dataPos++] = (byte) val;
                            break;
                        }

                        state++;
                        goto case 5;

                    case 5:
                        if (val < 0)
                            throw new Exception(string.Format("Record has an invalid checksum field ({0})", rec));
                        recBuf.CheckSum = (byte) val;
                        state++;
                        break;
                }

                checksum += val;
            }

            if (rem > 0 || state != 6)
                throw new Exception(string.Format("Invalid record ({0})", rec));

            recBuf.CheckSumValid = (checksum & 0xff) == 0;
        }
    }
}
