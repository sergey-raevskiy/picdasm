using System;
using System.Diagnostics;
using System.IO;

namespace picdasm
{
    internal class Program
    {
        private static string ResolveMem(uint f, uint a)
        {
            if (a == 0 && f == 0xf5)
            {
                return "TABLAT";
            }
            else if (a == 0)
            {
                return string.Format("__accessbnk(0x{0:X2})", f);
            }
            else
            {
                return string.Format("__curbnk(0x{0:X2})", f);
            }
        }

        private static void Disasm(byte[] prog)
        {
            var helper = new InstructionDecoderHelper(prog);

                var dec  =new PicInstructionDecoder(prog);

            while (true)
            {
                if (dec.IsMovLW(out byte literal))
                {
                    Console.WriteLine("W = 0x{0:X2}", literal);
                }
                else if (dec.IsMovWF(out byte addr, out AddrMode mode))
                {
                    if (mode == AddrMode.Access)
                    {
                        //Console.WriteLine()
                    }
                }
            }

            while (true)
            {
                Console.Write("_0x{0:X4}: ", helper.Pos());

                if (helper.Match("0000 1110 kkkk kkkk"))
                {
                    // MOVLW
                    Console.WriteLine("W = 0x{0:X2};", helper.Fields['k']);
                }
                else if (helper.Match("0110 111a ffff ffff"))
                {
                    // MOVWF
                    Console.WriteLine("{0} = W;", ResolveMem(helper.Fields['f'], helper.Fields['a']));
                }
                else if (helper.Match("1101 0`nnn nnnn nnnn"))
                {
                    // BRA
                    Console.WriteLine("goto _0x{0:X4};", 2*helper.Fields['n'] + helper.Pos());
                }
                else if (helper.Match("0000 0000 0000 0100"))
                {
                    // CLRWDT
                    Console.WriteLine("__clrwdt();");
                }
                else if (helper.Match("1110 1111 kkkk kkkk  1111 KKKK KKKK KKKK"))
                {
                    // GOTO
                    uint addr = helper.Fields['k'] + helper.Fields['K'] * 256;
                    Console.WriteLine("goto _0x{0:X4};", addr * 2);
                }
                else if (helper.Match("0000 0000 0000 0000") || helper.Match("1111 xxxx xxxx xxxx"))
                {
                    // NOP
                    Console.WriteLine("__nop();");
                }
                else if (helper.Match("0101 00da ffff ffff"))
                {
                    // MOVF
                    if (helper.Fields['a'] == 0 && helper.Fields['d'] == 0)
                    {
                        Console.WriteLine("W = __accessbnk(0x{0:X2});", helper.Fields['f']);

                    }
                    else
                    {
                        Console.WriteLine("__MOVF({0},{1},{2});", helper.Fields['f'], helper.Fields['d'], helper.Fields['a']);
                        return;
                    }
                }
                else if (helper.Match("0110 001a ffff ffff"))
                {
                    // CPFSEQ
                    if (helper.Fields['a'] == 0)
                    {
                        Console.WriteLine("if (!(__accessbnk(0x{0:X2}) == W))", helper.Fields['f']);

                    }
                    else
                    {
                        Console.WriteLine("CPFSEQ()");
                        return;
                    }
                }
                else if (helper.Match("0000 0000 0000 10nn"))
                {
                    // TBLRD
                    if (helper.Fields['n'] == 0)
                        Console.WriteLine("TABLAT = __pgmem(TBLPTR);");
                    else if (helper.Fields['n'] == 1)
                        Console.WriteLine("TABLAT = __pgmem(TBLPTR--);");
                    else if (helper.Fields['n'] == 2)
                        Console.WriteLine("TABLAT = __pgmem(TBLPTR++);");
                    else if (helper.Fields['n'] == 3)
                        Console.WriteLine("TABLAT = __pgmem(++TBLPTR);");
                    else
                    {
                        Debug.Assert(false);
                        return;
                    }
                }
                else if (helper.Match("xxxx xxxx xxxx xxxx"))
                {
                    Console.WriteLine(".dw 0x{0:X4}", helper.Fields['x']);
                    return;
                }
                //else if (helper.Match("xxxx xxxx"))
                //{
                //    Console.WriteLine(".db 0x{0:X2}", helper.Fields['x']);
                //}
                else if (helper.Eof())
                {
                    // EOF
                    break;
                }
                else
                {
                    throw new Exception("Failed to match bits");
                }
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

            // little endian ugly hack
            var mem = progMemImage.GetMem();
            for (int i = 0; i < mem.Length; i += 2)
            {
                byte tmp = mem[i];
                mem[i] = mem[i + 1];
                mem[i + 1] = tmp;
            }

            Disasm(mem);
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
