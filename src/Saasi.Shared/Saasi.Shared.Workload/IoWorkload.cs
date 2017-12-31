using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Saasi.Shared.Workload
{
    public class IoWorkload : IWorkload
    {
        private static readonly long _fileSize = 10L * 1024L * 1024L * 1024L; //10 G

        public async Task<ExecutionResult> Run(int time)
        {
            var startTime = System.DateTime.Now;
            var exceptions = false;

            try {
                await DiskIoProcess(time);
            } catch {
                exceptions = true;
            }
            
            return new ExecutionResult {
                TaskStartedAt = startTime,
                TaskFinishedAt = System.DateTime.Now,
                HasExceptions = exceptions,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString()
            };
        }

        public async Task DiskIoProcess(int time)
        {
            // simulate block i/o use
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            String st = Guid.NewGuid().ToString();
            String fileName = "write" + st + ".tmp";
            FileStream fs = new FileStream(fileName, FileMode.Create);
            fs.SetLength(_fileSize);
            StreamWriter sw = new StreamWriter(fs);
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                String s = GenerateRandomString(1000);
                await sw.WriteAsync(s);
                fs.Flush(true);
                // change sleep time to control block write speed
                //Thread.Sleep(3); 
            }
            sw.Dispose();

            fs.Dispose();
            var fi = new System.IO.FileInfo(fileName);
            fi.Delete();
        }

        private static string GenerateRandomString(int length)
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
