using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

    class Writer
    {
        public int PC { get; set; }

        int indent = 0;
        bool prespace;

        private HashSet<int> calls = new HashSet<int>();
        private HashSet<int> gotos = new HashSet<int>();

        class Line
        {
            public int addr;
            public string text;
        }

        private List<Line> lines = new List<Line>();

        public void RefCall(int addr)
        {
            calls.Add(addr);
        }

        public void RefGoto(int addr)
        {
            gotos.Add(addr);
        }

        public void IncIndent()
        {
            indent = 1;
        }

        public void PreSpace()
        {
            prespace = true;
        }

        public void WriteLine(string f, params object[] args)
        {
            if (prespace)
            {
                lines.Add(new Line { addr = -1, text = "" });
                prespace = false;
            }

            string text = "    ";
            if (indent != 0)
            {
                text += "    ";
                indent = 0;
            }

            text += string.Format(f, args);
            lines.Add(new Line { addr = PC, text = text });
        }

        public void Dump(TextWriter w)
        {
            foreach (var line in lines)
            {
                bool sk = false;
                if (calls.Contains(line.addr))
                {
                    w.WriteLine();
                    sk = true;
                    w.WriteLine("case 0x{0:X5}:", line.addr);
                }

                if (gotos.Contains(line.addr))
                {
                    if (!sk)
                        w.WriteLine();
                    w.WriteLine("_0x{0:X5}:", line.addr);
                }

                w.WriteLine(line.text);
            }
        }
    }

    internal class InstructionWriter : IPicInstructionExecutor
    {
        private readonly Writer o;
        private readonly Context c;

        private static string ResolveAbsAddr(int addr)
        {
            string sfrName = Pic18Sfr.LookupSfr(addr);

            if (sfrName != null)
            {
                return sfrName;
            }
            else
            {
                return string.Format("Mem[0x{0:X3}]", addr);
            }
        }

        private string ResolveAddr(byte addr, AccessMode access)
        {
            if (access == AccessMode.Access)
            {
                int absAddr = (addr - 96 + 0x0f60) & 0xfff;

                return ResolveAbsAddr(absAddr);
            }
            else
            {
                return string.Format("Mem[BSR << 8 | 0x{0:X2}]", addr);
            }
        }

        private string ResolveBit(byte addr, AccessMode access, int bit)
        {
            if (access == AccessMode.Access)
            {
                int absAddr = (addr - 96 + 0x0f60) & 0xfff;

                string s = Pic18Sfr.LookupBit(absAddr * 8 + bit);
                if (s != null)
                    return s;
            }

            return bit.ToString();
        }


        public InstructionWriter(Context c)
        {
            this.o = new Writer();
            this.c = c;

            o.RefCall(0);
            o.RefCall(0x8);
            o.RefCall(0x18);
        }

        public void SetPc(int pc)
        {
            o.PC = pc;
        }

        public void Dump(TextWriter w)
        {
            w.WriteLine("void _FUNC_(int _pfn)");
            w.WriteLine("{");
            w.WriteLine("switch (_pfn) {");
            o.Dump(w);
            w.WriteLine("} /* switch (_pfn) { */");
            w.WriteLine("}");
        }

        public void NOP()
        {
            o.WriteLine("_nop();");
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
            o.PreSpace();
            o.WriteLine("W = 0x{0:X2};", literal);
        }

        public void MOVF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.PreSpace();
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

        public void IORWF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W |= {0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} |= W;", ResolveAddr(addr, access));
            }
        }

        public void CPFSEQ(byte addr, AccessMode access)
        {
            o.WriteLine("if ({0} != W)", ResolveAddr(addr, access));
            o.IncIndent();
        }

        public void CLRF(byte addr, AccessMode access)
        {
            o.WriteLine("{0} = 0;", ResolveAddr(addr, access));
        }

        public void MOVWF(byte addr, AccessMode access)
        {
            o.WriteLine("{0} = W;", ResolveAddr(addr, access));
        }

        public void MOVFF(int source, int dest)
        {
            o.WriteLine("{0} = {1};", ResolveAbsAddr(dest), ResolveAbsAddr(source));
        }

        public void BRA(int off)
        {
            int addr = c.PC + 2 * off;
            o.WriteLine("goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void RCALL(int off)
        {
            int addr = c.PC + 2*off;
            o.WriteLine("_FUNC_(0x{0:X5});", addr);
            o.RefCall(addr);
        }

        public void BZ(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (Z) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BNZ(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (!Z) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BC(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (C) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BNC(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (!C) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BOV(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (O) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BNOV(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (!O) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BN(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (N) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void BNN(int off)
        {
            int addr = (c.PC + 2 * off);
            o.WriteLine("if (!N) goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void GOTO(int addr)
        {
            addr *= 2;
            o.WriteLine("goto _0x{0:X5};", addr);
            o.PreSpace();
            o.RefGoto(addr);
        }

        public void LFSR(int f, int k)
        {
            o.PreSpace();
            o.WriteLine("LFSR{0} = 0x{1:X2};", f, k);
        }

        HashSet<string> unks = new HashSet<string>()
        {
            "0001",
"0002",
"0017",
"0018",
"0019",
"001A",
"001B",
"001C",
"001D",
"001E",
"001F",
"0020",
"0021",
"0022",
"0023",
"0024",
"0025",
"0026",
"0027",
"0028",
"0029",
"002A",
"002B",
"002C",
"002D",
"002E",
"002F",
"0030",
"0031",
"0032",
"0033",
"0034",
"0035",
"0038",
"0039",
"003B",
"003C",
"003D",
"003E",
"003F",
"0040",
"0041",
"0042",
"0043",
"0044",
"0047",
"0048",
"004A",
"0050",
"0051",
"0052",
"0055",
"0056",
"0058",
"0059",
"005A",
"005B",
"005C",
"005D",
"005E",
"005F",
"0060",
"0061",
"0062",
"0063",
"0064",
"0065",
"0066",
"0067",
"0068",
"0069",
"006B",
"006C",
"006F",
"0072",
"0075",
"0078",
"0079",
"007C",
"007D",
"0080",
"0081",
"0082",
"0088",
"0090",
"0092",
"0094",
"0097",
"009C",
"009F",
"00A1",
"00A5",
"00AC",
"00AD",
"00BA",
"00BB",
"00C0",
"00C6",
"00CE",
"00CF",
"00D0",
"00D1",
"00D2",
"00D9",
"00DA",
"00DE",
"00DF",
"00E0",
"00E1",
"00E8",
"00EB",
        };

        public void Unknown(byte hiByte, byte loByte)
        {
            string kk = string.Format("{0:X2}{1:X2}", hiByte, loByte);
            o.WriteLine("_unk(0x{0:X2}{1:X2});", hiByte, loByte);

            if (!unks.Contains(kk))
                throw new NotImplementedException(kk);
        }

        public void CALLW()
        {
            o.WriteLine("_callw_();");
        }

        public void NOPEX(byte hiByte, byte loByte)
        {
            o.WriteLine("_nopex(0x{0:X2}{1:X2});", hiByte, loByte);
        }

        public void CPFSLT(byte addr, AccessMode access)
        {
            o.WriteLine("if ({0} >= W)", ResolveAddr(addr, access));
            o.IncIndent();
        }

        public void BTG(byte addr, int bit, AccessMode access)
        {
            o.WriteLine("{0} ^= (1 << {1});", ResolveAddr(addr, access), ResolveBit(addr, access, bit));
        }

        public void BSF(byte addr, int bit, AccessMode access)
        {
            o.WriteLine("{0} |= (1 << {1});", ResolveAddr(addr, access), ResolveBit(addr, access, bit));
        }

        public void BCF(byte addr, int bit, AccessMode access)
        {
            o.WriteLine("{0} &= ~(1 << {1});", ResolveAddr(addr, access), ResolveBit(addr, access, bit));
        }

        public void SETF(byte addr, AccessMode access)
        {
            o.WriteLine("{0} = 0xFF;", ResolveAddr(addr, access));
        }

        public void XORWF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W ^= {0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} ^= W;", ResolveAddr(addr, access));
            }
        }

        public void INCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.PreSpace();
                o.WriteLine("W = {0} + 1;", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0}++;", ResolveAddr(addr, access));
            }
        }

        public void INFSNZ(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("if ((W = {0} + 1) == 0)", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("if (++{0} == 0)", ResolveAddr(addr, access));
            }

            o.IncIndent();
        }

        public void DECFSZ(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("if ((W = {0} - 1) != 0)", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("if (--{0} != 0)", ResolveAddr(addr, access));
            }

            o.IncIndent();
        }

        public void RESET()
        {
            o.WriteLine("__reset();");
        }

        public void BTFSS(byte addr, int bit, AccessMode access)
        {
            o.WriteLine("if (!({0} & (1 << {1})))", ResolveAddr(addr, access), ResolveBit(addr, access, bit));
            o.IncIndent();
        }

        public void BTFSC(byte addr, int bit, AccessMode access)
        {
            o.WriteLine("if ({0} & (1 << {1}))", ResolveAddr(addr, access), ResolveBit(addr, access, bit));
            o.IncIndent();
        }

        public void TSTFSZ(byte addr, AccessMode access)
        {
            o.WriteLine("if ({0})", ResolveAddr(addr, access));
            o.IncIndent();
        }

        public void XORLW(byte literal)
        {
            o.WriteLine("W ^= 0x{0:X2};", literal);
        }

        public void SUBWF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.PreSpace();
                o.WriteLine("W = {0} - W;", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0}--;", ResolveAddr(addr, access));
            }
        }

        public void CPFSGT(byte addr, AccessMode access)
        {
            o.WriteLine("if ({0} <= W)", ResolveAddr(addr, access));
            o.IncIndent();
        }

        public void ANDLW(byte literal)
        {
            o.WriteLine("W &= 0x{0};", literal);
        }

        public void ANDWF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W &= {0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} &= W;", ResolveAddr(addr, access));
            }
        }

        public void CALL(int addr, CallReturnOpMode mode)
        {
            addr *= 2;
            o.WriteLine("_FUNC_(0x{0:X5});", addr);
            o.PreSpace();
            o.RefCall(addr);
        }

        public void SUBLW(byte literal)
        {
            o.WriteLine("W = 0x{0:X2} - W;", literal);
        }

        public void IORLW(byte literal)
        {
            o.WriteLine("W |= 0x{0:X2};", literal);
        }

        public void RETLW(byte literal)
        {
            o.WriteLine("_return_(RETLW(0x{0:X2}));", literal);
            o.PreSpace();
        }

        public void MULLW(byte literal)
        {
            o.WriteLine("PROD = W * 0x{0:X2};", literal);
        }

        public void ADDLW(byte literal)
        {
            o.WriteLine("W += 0x{0:X2};", literal);
        }

        public void POP()
        {
            o.WriteLine("__pop();");
        }

        public void DECF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.PreSpace();
                o.WriteLine("W = {0} - 1;", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0}--;", ResolveAddr(addr, access));
            }
        }

        public void RETURN(CallReturnOpMode mode)
        {
            o.WriteLine("return;");
            o.PreSpace();
        }

        public void TBLWT(TableOpMode mode)
        {
            switch (mode)
            {
                case TableOpMode.None:
                    o.WriteLine("__tblwt(TBLPTR, TABLAT);");
                    break;
                case TableOpMode.PostIncrement:
                    o.WriteLine("__tblwt(TBLPTR++, TABLAT);");
                    break;
                case TableOpMode.PostDecrement:
                    o.WriteLine("__tblwt(TBLPTR--, TABLAT);");
                    break;
                case TableOpMode.PreIncrement:
                    o.WriteLine("__tblwt(++TBLPTR, TABLAT);");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void MOVLB(int literal)
        {
            o.WriteLine("BSR = 0x{0:X2};", literal);
        }

        public void MULWF(byte addr, AccessMode access)
        {
            o.WriteLine("PROD = W * {0};", ResolveAddr(addr, access));
        }

        public void COMF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = ~{0};", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = ~{0};", ResolveAddr(addr, access));
            }
        }

        public void RRCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = _rot_(RRCF, {0});", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = _rot_(RRCF, {0});", ResolveAddr(addr, access));
            }
        }

        public void RLCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = _rot_(RLCF, {0});", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = _rot_(RLCF, {0});", ResolveAddr(addr, access));
            }
        }

        public void SWAPF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = _rot_(SWAPF, {0});", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = _rot_(SWAPF, {0});", ResolveAddr(addr, access));
            }
        }

        public void INCFSZ(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("if ((W = {0} - 1) == 0)", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("if (--{0} == 0)", ResolveAddr(addr, access));
            }

            o.IncIndent();
        }

        public void RRNCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = _rot_(RRNCF, {0});", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = _rot_(RRNCF, {0});", ResolveAddr(addr, access));
            }
        }

        public void RLNCF(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = _rot_(RLNCF, {0});", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = _rot_(RLNCF, {0});", ResolveAddr(addr, access));
            }
        }

        public void DCFSNZ(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("if ((W = {0} - 1) == 0)", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("if (--{0} == 0)", ResolveAddr(addr, access));
            }

            o.IncIndent();
        }

        public void SUBFWB(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W -= ({0} + C);", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} = W  - {0} - C;", ResolveAddr(addr, access));
            }
        }

        public void SUBWFB(byte addr, DestinationMode dest, AccessMode access)
        {
            if (dest == DestinationMode.W)
            {
                o.WriteLine("W = {0} - W - C;", ResolveAddr(addr, access));
            }
            else
            {
                o.WriteLine("{0} -= (W + C);", ResolveAddr(addr, access));
            }
        }

        public void RETFIE(CallReturnOpMode mode)
        {
            o.WriteLine("_return_(RETFIE);");
        }

        public void NEGF(byte addr, AccessMode access)
        {
            o.WriteLine("{0} = -{0};", ResolveAddr(addr, access));
        }

        public void SLEEP()
        {
            o.WriteLine("_sleep_();");
        }

        public void MOVSF(int src, int dst)
        {
            o.WriteLine("{1} = Mem[FSR2 + 0x{0:X2}]", src, ResolveAbsAddr(dst));
        }

        public void MOVSS(int src, int dst)
        {
            o.WriteLine("Mem[FSR2 + 0x{1:X2}] = Mem[FSR2 + 0x{0:X2}]", src, dst);
        }

        public void PUSH()
        {
            o.WriteLine("_push_();");
        }

        public void DAW()
        {
            o.WriteLine("W = _rot_(DAW, W);");
        }

        public void ADDFSR(int n, int k)
        {
            o.WriteLine("FSR{0} += 0x{1:X2};", n, k);
        }

        public void ADDULNK(int k)
        {
            o.WriteLine("_return_(ULNK(FSR2 += k));", k);
        }

        public void PUSHL(byte l)
        {
            o.WriteLine("Mem[FSR2--] = 0x{0:X2};", l);
        }

        public void SUBFSR(int n, int k)
        {
            o.WriteLine("FSR{0} -= 0x{1:X2};", n, k);
        }

        public void SUBULNK(int k)
        {
            o.WriteLine("_return_(ULNK(FSR2 -= k));", k);
        }

        public void EMULEN()
        {
            o.WriteLine("_emulen_();");
        }

        public void EMULDIS()
        {
            o.WriteLine("_emuldis_();");
        }
    }

    interface IPicInstructionFetcher
    {
        void FetchInstruciton(out byte hi, out byte lo);
    }

    internal class Program
    {
        private static void Disasm(byte[] prog)
        {
            var ctx = new Context(prog);

            var qq = new InstructionWriter(ctx);
            var dec = new PicInstructionDecoder(ctx, qq);

            while (ctx.PC < prog.Length)
            {
                qq.SetPc(ctx.PC);
                dec.DecodeAt();
            }

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
