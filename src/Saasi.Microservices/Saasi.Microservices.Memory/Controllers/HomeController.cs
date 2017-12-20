using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Saasi.Microservices.Memory.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {

        // HTTP GET api/memory
        [HttpGet("memory")]
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
