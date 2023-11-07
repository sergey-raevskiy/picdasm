using System;
using System.Collections.Generic;
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

            List<string> unk = new List<string>()
            {
                ".dw 0x001B",
                ".dw 0x0048",
                ".dw 0x0002",
                ".dw 0x00C0",
                ".dw 0x0082",
                ".dw 0x00F1",
                ".dw 0x0080",
                ".dw 0x0040",
                ".dw 0x0082",
                ".dw 0x0090",
                ".dw 0x0001",
                ".dw 0x0075",
            };

            while (true)
            {
                if (!dec.DecodeAt(buf, pc))
                    //throw new Exception(string.Format("Failed to decode instruciton at (0x{0:X4})", pc));
                    break;

                if (buf.InstrucitonKind == PicInstructionKind.Misc)
                {
                    Console.WriteLine("{0}", buf.MiscInstruction);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Literal)
                {
                    Console.WriteLine("{0}", buf.LiteralInstruction.OpCode);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Alu)
                {
                    Console.WriteLine("{0}", buf.AluInstruction.OpCode);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Alu2)
                {
                    Console.WriteLine("{0}", buf.AluInstruction2.OpCode);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.BRA)
                {
                    Console.WriteLine("{0}", PicInstructionKind.BRA);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.RCALL)
                {
                    Console.WriteLine("{0}", PicInstructionKind.RCALL);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.GOTO)
                {
                    Console.WriteLine("GOTO 0x{0:X4}", buf.GOTO.Addr);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.CALL)
                {
                    Console.WriteLine("GOTO 0x{0:X4}", buf.CALL.Addr);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.TBLRD)
                {
                    Console.WriteLine("TBLRD");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.TBLWR)
                {
                    Console.WriteLine("TBLWR");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.MOVFF)
                {
                    Console.WriteLine("MOVFF");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.ConditionalBranch)
                {
                    Console.WriteLine("{0}", buf.ConditionalBranch.OpCode);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.LFSR)
                {
                    Console.WriteLine("LFSR");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.BitInstruction)
                {
                    Console.WriteLine("{0}", buf.BitInstruction.OpCode);
                }
                else if (buf.InstrucitonKind == PicInstructionKind.RETURN)
                {
                    Console.WriteLine("RETURN");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.RETFIE)
                {
                    Console.WriteLine("RETFIE");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.NOPEx)
                {
                    Console.WriteLine("NOPEX");
                }
                else if (buf.InstrucitonKind == PicInstructionKind.Unknown)
                {
                    string instr = string.Format(".dw 0x{0:X2}{1:X2}", buf.HiByte, buf.LoByte);

                    Console.WriteLine(instr);
                    if (!unk.Contains(instr))
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
