using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Memory_microservice.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult memory(int time)
        {
            memoryProcess(time);
            return Content("OK");
        }

        public void memoryProcess(int time)
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
                byte[] b = new byte[30];
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
