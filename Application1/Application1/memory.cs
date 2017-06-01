using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace memorySpace
{
    public class memory
    {
        private static String Timeout;
        public memory(String timeout)
        {
            Timeout = timeout;
        }
        public void Fun()
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(Convert.ToInt16(Timeout));
            Console.WriteLine("MEMORY service start." + Convert.ToString(currentTime));
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                double[,] a = new double[5000, 5000];
            }
            Console.WriteLine("MEMORY service end." + Convert.ToString(System.DateTime.Now));
        }
    }
}
