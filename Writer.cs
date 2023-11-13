using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace picdasm
{
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
            public bool prespace;
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
            bool prespace = this.prespace;
            this.prespace = false;

            string text = "    ";
            if (indent != 0)
            {
                text += "    ";
                indent = 0;
            }

            text += string.Format(f, args);
            lines.Add(new Line { prespace = prespace, addr = PC, text = text });
        }

        private readonly Dictionary<int, string[]> Rewrites = new Dictionary<int, string[]>();

        public void Rewrite(int start, int end, string[] lines)
        {
            for (int i = start; i < end; i += 2)
            {
                if (Rewrites.ContainsKey(i))
                {
                    throw new Exception("Rewrite collision");
                }

                Rewrites.Add(i, lines);
                lines = null;
            }
        }

        private IEnumerable<Line> Lines()
        {
            foreach (var line in lines)
            {
                if (Rewrites.ContainsKey(line.addr))
                {
                    var qq = Rewrites[line.addr];
                    if (qq != null)
                    {
                        foreach (var ll in qq)
                        {
                            yield return new Line() { addr = line.addr, text =  ll != null ? "    " + ll : "" };
                        }
                    }
                    else if (calls.Contains(line.addr) || gotos.Contains(line.addr))
                    {
                        yield return new Line() { addr = -1, text = string.Format(" /* warn: ref _0x{0:X5} is broken */", line.addr) };
                    }
                }
                else
                {
                    yield return line;
                }
            }
        }

        public void Dump(TextWriter w)
        {
            int prev = -1;
            foreach (var line in Lines())
            {
                if (line.prespace)
                    w.WriteLine();

                if (line.addr != prev && line.text!="")
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

                    prev = line.addr;
                }

                w.WriteLine(line.text);
            }
        }
    }
}
