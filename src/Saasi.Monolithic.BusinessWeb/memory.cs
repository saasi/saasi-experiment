using System;
using System.Collections.Generic;
using System.Threading;

namespace BusinessFunction
{
    public class memory
    {
        private int timetorun;
        public memory(String timetorun)
        {
            this.timetorun = Convert.ToInt16(timetorun);
        }
        public void Fun(object state)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(timetorun);
            Console.WriteLine("Memory Start." + Convert.ToString(currentTime));
            List<byte[]> alist = new List<byte[]>();
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                byte[] b = new byte[30];
                alist.Add(b);

                i++;
                if (i == 2000)
                {
                    Thread.Sleep(50); // Change the wait time here.
                    i = 0;

                }


            }
            alist.Clear();
            alist = null;
            GC.Collect();
            alist = null;
            Console.WriteLine("Memory Done." + Convert.ToString(System.DateTime.Now));
        }
    }
}
