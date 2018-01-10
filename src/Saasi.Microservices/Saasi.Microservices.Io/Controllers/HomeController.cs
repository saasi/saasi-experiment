using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using Saasi.Shared.Workload;

namespace Saasi.Microservices.Io.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {

        // HTTP GET api/io?time=xxx
        [HttpGet("io")]
        public async Task<string> Run(int time)
        {
            DateTime currentTime = System.DateTime.Now;
            Guid id = Guid.NewGuid();
            Console.WriteLine(id.ToString() + ":Start." + Convert.ToString(currentTime));
  
            var task = new IoWorkload();
            await task.Run(time);

            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));

            return $"OK. Disk I/O job done. ";
        }

    }
}
