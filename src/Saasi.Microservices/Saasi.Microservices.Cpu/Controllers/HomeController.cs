using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading;
using Saasi.Shared.Workload;

namespace Saasi.Microservices.Cpu.Controllers
{
    [Route("api/")]
    public class HomeController : Controller
    {
        // GET api/cpu
        [HttpGet("cpu")]
        public async Task<string> Run(int time)
        {
            DateTime currentTime = System.DateTime.Now;
            Guid id = Guid.NewGuid();
            Console.WriteLine(id + ":Start." + Convert.ToString(currentTime));
            var task = new CpuWorkload();
            await task.Run(time);
            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));
            return $"OK. CPU task finished. Seconds run = {time}.";
        }

    }
}
