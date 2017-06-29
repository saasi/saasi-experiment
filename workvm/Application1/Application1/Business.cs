using Application2;
using cpuSpace;
using ioSpace;
using memorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application1
{
    public class Business
    {
        private String io;
        private String memory;
        private String cpu;
        private String timeout;

        public Business(String[] order)
        {
            this.cpu = order[0];
            this.io = order[1];
            this.memory = order[2];
            this.timeout = order[3];
        }

        public void Fun()
        {
            if (this.cpu.Equals("1"))
            {
                cpu CPU = new cpu(this.timeout);
                new Thread(CPU.Fun).Start();
            }           


            if (this.io.Equals("1"))
            {
                io IO = new io(this.timeout);
                new Thread(IO.Fun).Start();
            }

            if (this.memory.Equals("1"))
            {
                memory MEM = new memory(this.timeout);
                new Thread(MEM.Fun).Start();
            }
        }
    }
}
