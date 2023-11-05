using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace picdasm
{
    class PicProgMemImage
    {
        private List<byte> memory = new List<byte>();
        private List<bool> memInit = new List<bool>();

        public byte[] GetMem()
        {
            return memory.ToArray();
        }

        public int LastInit()
        {
            return memInit.LastIndexOf(true);
        }

        public PicProgMemImage(int memSize)
        {
            memory = new List<byte>(memSize);
            memory.AddRange(Enumerable.Repeat((byte)0, memSize));

            memInit = new List<bool>(memSize);
            memInit.AddRange(Enumerable.Repeat(false, memSize));
        }

        public bool AnyMemInit(int address, int len)
        {
            if (memory.Count < address + len)
            {
                // TODO: auto expand
                throw new ArgumentException();
            }

            for (int i = 0; i < len; i++)
            {
                if (memInit[address + i])
                    return true;
            }

            return false;
        }

        public void Write(int address, byte[] data, int len)
        {
            Debug.Assert(data.Length >= len);

            if (memory.Count < address + len)
            {
                // TODO: auto expand
                throw new ArgumentException();
            }

            for (int i = 0; i < len; i++)
            {
                memory[address + i] = data[i];
                memInit[address + i] = true;
            }
        }
    }
}
