using System.Collections.Generic;
using System.IO;

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
}
