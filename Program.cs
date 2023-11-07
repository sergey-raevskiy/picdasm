using System;
using System.Diagnostics;
using System.IO;

namespace picdasm
{
    internal class Program
    {
        private static void Disasm(byte[] prog)
        {
            var dec = new PicInstructionDecoder(prog);
            PicInstructionBuf buf = new PicInstructionBuf();
            int pc = 0;

            while (true)
            {
                if (!dec.DecodeAt(buf, pc))
                    throw new Exception(string.Format("Failed to decode instruciton at (0x{0:X4})", pc));

                if (buf.InstrucitonKind == PicInstructionKind.Literal)
                {
                    Console.WriteLine("{0}", buf.LiteralInstruction);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Alu)
                {
                    Console.WriteLine("{0}", buf.AluInstruction);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Unknown)
                {
                    Console.WriteLine(".dw 0x{0:X2}{1:X2}", buf.HiByte, buf.LoByte);
                    return;
                }
                else
                {
                    Debug.Assert(false);
                }

                pc += buf.InstructionLength;
            }
        }

        private static void Run(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("usage: picdasm HEX_FILE");

            string fileName = args[0];

            var progMemImage = new PicProgMemImage(0x20000);

            using (var reader = File.OpenText(fileName))
            {
                var hexReader = new IntelHexReader(reader);
                var recordBuf = new IntelHexRecordBuf();
                int addressOffset = 0;

                while (true)
                {
                    hexReader.ReadRecord(recordBuf);

                    //IntelHexRecordDumper.DumpRecordConsole(recordBuf);

                    if (!recordBuf.CheckSumValid)
                        throw new Exception("Record has an invalid checksum");

                    if (recordBuf.RecordType == IntelHexRecordType.EndOfFile)
                    {
                        break;
                    }
                    else if (recordBuf.RecordType == IntelHexRecordType.Data)
                    {
                        int address = addressOffset + recordBuf.Address;
                        int len = recordBuf.Length;
                        byte[] data = recordBuf.DataBuf;

                        if (address >= 0x300000)
                        {
                            // Skip configuration bytes for now.
                            continue;
                        }

                        if (progMemImage.AnyMemInit(address, len))
                            throw new Exception("Trying to overwrite already initialized code");

                        progMemImage.Write(address, data, len);
                    }
                    else if (recordBuf.RecordType == IntelHexRecordType.ExtendedLinearAddress)
                    {
                        if (recordBuf.Length != 2)
                            throw new Exception("Invalid ExtendedLinearAddress record");

                        addressOffset = (recordBuf.DataBuf[0] * 256 + recordBuf.DataBuf[1]) * 65536;
                    }
                }
            }

            Console.WriteLine("Last initialized address: {0:X6}", progMemImage.LastInit());

            Disasm(progMemImage.GetMem());
        }

        static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}", ex.Message);
                return 1;
            }
        }
    }
}
