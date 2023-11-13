using System;
using System.Diagnostics;
using System.IO;

namespace picdasm
{
    internal class Program
    {
        private static void DisasmCore(byte[] prog, Context ctx, IPicInstructionExecutorNoDispatch qq)
        {
            var dec = new PicInstructionDecoder(null);
            PicInstructionBuf buf = new PicInstructionBuf();

            while (ctx.PC < prog.Length)
            {
                int pc = ctx.PC;

                ctx.Fetch(buf);

                PicInstrucitonType instr = dec.Decode(buf);
                if (instr > PicInstrucitonType.LongStart)
                {
                    ctx.FetchLong(buf);
                    qq.Exec(pc, buf, instr);
                    ctx.PC += 4;
                }
                else
                {
                    qq.Exec(pc, buf, instr);
                    ctx.PC += 2;
                }
            }
        }

        private static void Disasm(byte[] prog)
        {
            var ctx = new Context(prog);
            var qq = new InstructionWriter(ctx);

            DisasmCore(prog, ctx, new Driver(qq));

            ctx = new Context(prog);    
            var qq2 = new XorSwitchMetaInstructionProcessor(qq.o);
            DisasmCore(prog, ctx, qq2);

            ctx = new Context(prog);
            var qq3 = new ImmediateSwitchInstructionProcessor(qq.o);
            DisasmCore(prog, ctx, new Driver(qq3));


            qq.Dump(Console.Out);
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

            Console.Error.WriteLine("Last initialized address: {0:X6}", progMemImage.LastInit());

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
