using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Saasi.Shared.Workload
{
    public class CpuWorkload : IWorkload
    {
        public async Task<ExecutionResult> Run(int time)
        {
            var startTime = DateTime.Now;
            var exceptions = false;
            long loops = 0;
            try {
                loops = await CpuProcess(time);
            } catch {
                exceptions = true;
            }
            

            return new ExecutionResult {
                HasExceptions = exceptions,
                TaskStartedAt = startTime,
                TaskFinishedAt = DateTime.Now,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString(),
                ExecutedLoops = loops,
                ThreadsCount = Process.GetCurrentProcess().Threads.Count
            };
        }

        private async Task<long> CpuProcess(int time)
        {
            // simulate cpu bound operation for `time` seconds
            for (var i = 1; i <= time; ++i) {
                for (int k = 0; k < 10000; ++k) {
                    string comparestring1 = StringDistance.GenerateRandomString(1000);
                }
                await Task.Delay(100);
            }
            return time * 10000L;
        }

        class StringDistance
        {
            public static string GenerateRandomString(int length)
            {
                var r = new Random((int)DateTime.Now.Ticks);
                var sb = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    int c = r.Next(97, 123);
                    sb.Append(Char.ConvertFromUtf32(c));
                }
                return sb.ToString();
            }
        }
    }
}
