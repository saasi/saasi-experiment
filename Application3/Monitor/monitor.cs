using System;
using System.Collections.Generic;

namespace Monitor
{
    class monitor
    {
        private static double IO_LIMIT;
        private static double CPU_LIMIT;
        private static double MEMORY_LIMIT;

        public static void Main(string[] args)
        {
            while (true)
            {

            }
        }

        public void ScaleOut()
        { }

        public void monitorUsage()
        {

        }

        public void IO_Queue()
        {

        }

        public Dictionary<string, string> getContainerList()
        {
            //call docker api to get each container's image
            return new Dictionary<string, string> ();
        }
    }
}