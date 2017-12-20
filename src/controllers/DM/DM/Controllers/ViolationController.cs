using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DM.Controllers
{
    public class ViolationController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(String bmsGuid)
        {
            writeBmsViolation(bmsGuid);

            return new JsonResult(new Dictionary<string, string> { { "status", "ok" } });

        }
        public static void writeBmsViolation(string bmsguid)
        {
            StreamWriter sw = System.IO.File.AppendText("data/business-violation.txt");
            sw.WriteLine(bmsguid + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }
    }
}
