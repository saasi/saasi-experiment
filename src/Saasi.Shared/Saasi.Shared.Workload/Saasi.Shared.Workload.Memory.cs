using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Saasi.Shared.Workload
{
    public class GenerateMemoryWorkload
    {
        public string Run(int time)
        {
            MemoryProcess(time);
            return "OK";
        }

        public void MemoryProcess(int time)
        {
            //simulate memory use
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Guid id = Guid.NewGuid();
            Console.WriteLine(id + ":Start." + Convert.ToString(currentTime));
            //List<IntPtr> alist = new List<IntPtr>();
            List<byte[]> alist = new List<byte[]>();
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                byte[] b = new byte[1024];
                alist.Add(b); // Change the size here.

                i++;
                if (i == 2000)
                {
                    Thread.Sleep(200); // Change the wait time here to control memory usage.
                    i = 0;

                }
            }
            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));
            alist.Clear();
            alist = null;
            GC.Collect(); // release memory
        }
    }
}
