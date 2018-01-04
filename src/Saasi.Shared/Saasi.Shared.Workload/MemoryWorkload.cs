using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Saasi.Shared.Workload
{
    public class MemoryWorkload : IWorkload
    {
        public async Task<ExecutionResult> Run(int time)
        {
            var startTime = DateTime.Now;
            var exceptions = false;

            try {
                await MemoryProcess(time);
            } catch {
                exceptions = true;
            }

            return new ExecutionResult {
                HasExceptions = exceptions,
                TaskFinishedAt = System.DateTime.Now,
                TaskStartedAt = startTime,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString()
            };
        }

        public async Task MemoryProcess(int time)
        {
            //simulate memory use
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);

            //List<IntPtr> alist = new List<IntPtr>();
            List<byte[]> alist = new List<byte[]>();
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                byte[] b = new byte[1024];
                alist.Add(b); // Change the size here.

                i++;
                if (i == 500)
                {
                    await Task.Delay(200);
                     // Change the wait time here to control memory usage.
                    i = 0;
                    
                }
            }
            alist.Clear();
            alist = null;
            GC.Collect(); // release memory
        }
    }
}
