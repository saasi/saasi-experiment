using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Saasi.Shared.Workload
{
    public class GenerateCpuWorkload
    {
        public string Run(int time)
        {
            CpuProcess(time);
            return $"OK. CPU task finished. Seconds run = {time}.";
        }

        private void CpuProcess(int time)
        {
            // simulate cpu bound operation for `time` seconds
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Guid id = Guid.NewGuid();
            Console.WriteLine(id + ":Start." + Convert.ToString(currentTime));
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                string comparestring1 = StringDistance.GenerateRandomString(1000);
                i++;
                if (i == 50)
                {
                    Thread.Sleep(30); // Change the wait time here to adjust cpu usage.
                    i = 0;
                }
            }
            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));
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
