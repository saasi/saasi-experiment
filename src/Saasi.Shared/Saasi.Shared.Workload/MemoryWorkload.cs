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
            long result = 0;
            try {
                result = await MemoryProcess(time);
            } catch {
                exceptions = true;
            }

            return new ExecutionResult {
                HasExceptions = exceptions,
                TaskFinishedAt = System.DateTime.Now,
                TaskStartedAt = startTime,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString(),
                ExecutedLoops = result
            };
        }

        public async Task<long> MemoryProcess(int time)
        {
            // simulate memory use (1s = 2500 rounds)

            List<byte[]> alist = new List<byte[]>();
            int i = 0;
            for (var j = 0; j < time; ++j){
                for (i = 1; i <= 1000; ++i) {
                    byte[] b = new byte[1024];
                    alist.Add(b); 
                }
                await Task.Delay(500);
            }
            await Task.Delay(500*time);

            alist.Clear();
            alist = null;
            GC.Collect(); // release memory
            return time * 2500;
        }
    }
}
