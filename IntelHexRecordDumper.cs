using System;
using System.IO;

namespace picdasm
{
    class IntelHexRecordDumper
    {
        static class DumpColors
        {
            public const ConsoleColor Start = ConsoleColor.DarkGray;
            public const ConsoleColor Length = ConsoleColor.DarkGray;
            public const ConsoleColor Address = ConsoleColor.Gray;
            public const ConsoleColor RecordType = ConsoleColor.DarkGray;
            public const ConsoleColor DataOdd = ConsoleColor.Gray;
            public const ConsoleColor DataEven = ConsoleColor.Gray;
            public const ConsoleColor ValidChecksum = ConsoleColor.DarkGray;
            public const ConsoleColor InvalidChecksum = ConsoleColor.DarkYellow;
        }

        public static void DumpRecordConsole(IntelHexRecordBuf recordBuf)
        {
            var oldColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = DumpColors.Start;
                Console.Write(":");

                Console.ForegroundColor = DumpColors.Length;
                Console.Write("{0:X2}", recordBuf.Length);

                Console.ForegroundColor = DumpColors.Address;
                Console.Write("{0:X4}", recordBuf.Address);

                Console.ForegroundColor = DumpColors.RecordType;
                Console.Write("{0:X2}", (int)recordBuf.RecordType);

                for (int i = 0; i < recordBuf.Length; i++)
                {
                    Console.ForegroundColor = (i % 2) == 0 ? DumpColors.DataEven : DumpColors.DataOdd;
                    Console.Write("{0:X2}", recordBuf.DataBuf[i]);
                }

                Console.ForegroundColor = recordBuf.CheckSumValid ? DumpColors.ValidChecksum : DumpColors.InvalidChecksum;
                Console.Write("{0:X2}", recordBuf.CheckSum);

                Console.WriteLine();
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        public static void DumpRecord(TextWriter writer, IntelHexRecordBuf recordBuf)
        {
            writer.Write(":{0:X2}{1:X4}{2:X2}",
                       recordBuf.Length, recordBuf.Address, (int)recordBuf.RecordType);

            for (int i = 0; i < recordBuf.Length; i++)
            {
                writer.Write("{0:X2}", recordBuf.DataBuf[i]);
            }

            writer.Write("{0:X2}", recordBuf.CheckSum);

            writer.WriteLine();
        }
    }
}
