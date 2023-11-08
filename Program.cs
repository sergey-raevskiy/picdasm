using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace picdasm
{
    class InstructionWriter : IPicInstructionProcessor
    {
        private readonly TextWriter o;

        private string ResolveAddr(byte addr, AccessMode access)
        {
            if (access == AccessMode.Access)
            {
                return string.Format("ACCESS_BANK(0x{0:X2})", addr);
            }
            else
            {
                return string.Format("CURRENT_BANK(0x{0:X2})", addr);
            }
        }

        public InstructionWriter(TextWriter o)
        {
            this.o = o;
        }

        public void NOP()
        {
            o.WriteLine("__nop();");
        }

        public void CLRWDT()
        {
            o.WriteLine("clrwdt();");
        }

        public void TBLRD(TableOpMode mode)
        {
            switch (mode)
            {
                case TableOpMode.None:
                    o.WriteLine("TABLAT = ProgMem(TBLPTR);");
                    break;
                case TableOpMode.PostIncrement:
                    o.WriteLine("TABLAT = ProgMem(TBLPTR++);");
                    break;
                case TableOpMode.PostDecrement:
                    o.WriteLine("TABLAT = ProgMem(TBLPTR--);");
                    break;
                case TableOpMode.PreIncrement:
                    o.WriteLine("TABLAT = ProgMem(++TBLPTR);");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void MOVLW(byte literal)
        {
            o.WriteLine();
            o.WriteLine("W = 0x{0:X2};", literal);
        }

        public void MOVF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine();
                o.WriteLine("W = {0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = {1};", ResolveAddr(addr, access), ResolveAddr(addr, access));
            }
        }

        public void ADDWF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W += {0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} += W;", ResolveAddr(addr, access));
            }
        }

        public void ADDWFC(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W += {0} + C;", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} += W + C;", ResolveAddr(addr, access));
            }
        }

        public void CPFSEQ(byte addr, AccessMode mode)
        {
            o.WriteLine("if (!(W == {0}))", ResolveAddr(addr, mode));
        }

        public void MOVWF(byte addr, AccessMode mode)
        {
            o.WriteLine("{0} = W;", ResolveAddr(addr, mode));
        }

        public void MOVFF(int source, int dest)
        {
            o.WriteLine("MEM({0}) = MEM({1});", source, dest);
        }

        public void BRA(int offset)
        {
            o.WriteLine("goto {0};", offset);
        }

        public void GOTO(int offset)
        {
            o.WriteLine("goto {0};", offset);
        }

        public void Unknown(byte hiByte, byte loByte)
        {
            throw new NotImplementedException(string.Format("Unknown instruciton {0:X2}{1:X2}", hiByte, loByte));
        }
    }

    internal class Program
    {
        private static void Disasm(byte[] prog)
        {
            var dec = new PicInstructionDecoder(prog, new InstructionWriter(Console.Out));
            int pc = 0;

            while (true)
            {
                int len = dec.DecodeAt(pc);
                if (len == 0)
                    break;

                pc+= len;
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
