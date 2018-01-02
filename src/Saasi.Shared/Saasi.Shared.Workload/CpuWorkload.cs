using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Saasi.Shared.Workload
{
    public class CpuWorkload : IWorkload
    {
        public async Task<ExecutionResult> Run(int time)
        {
            var startTime = DateTime.Now;
            var exceptions = false;

            try {
                await CpuProcess(time);
            } catch {
                exceptions = true;
            }

            return new ExecutionResult {
                HasExceptions = exceptions,
                TaskStartedAt = startTime,
                TaskFinishedAt = DateTime.Now,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString()
            };
        }

        private async Task CpuProcess(int time)
        {
            // simulate cpu bound operation for `time` seconds
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                string comparestring1 = StringDistance.GenerateRandomString(1000);
                i++;
                if (i == 500)
                {
                    await Task.Delay(30); // Change the wait time here to adjust cpu usage.
                    i = 0;
                }
            }
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
