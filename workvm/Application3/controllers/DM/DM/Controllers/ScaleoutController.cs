using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Diagnostics;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DM.Controllers
{
    public class ScaleoutController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string bmsGuid, int bmsCount)
        {
            Console.WriteLine(bmsGuid);
            scaleOut("bms", bmsCount);
            writeRecord(bmsGuid);

            return View();
        }
        public static void writeRecord(string bmsguid) //record bms scaleout
        {
            StreamWriter sw = System.IO.File.AppendText("data/business-scaleout.txt");
            sw.WriteLine(bmsguid.ToString() + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }
        public static void scaleOut(string type, int bmsNum)
        {
            //scalebms
            if (type.Equals("bms"))
            {
                Console.WriteLine("scaleout bms");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scalebms1.sh " + bmsNum }; //scale bms
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

        }







    }

}
