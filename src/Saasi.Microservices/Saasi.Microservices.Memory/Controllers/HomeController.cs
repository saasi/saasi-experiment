using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Saasi.Shared.Workload;

namespace Saasi.Microservices.Memory.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {

        // HTTP GET api/memory
        [HttpGet("memory")]
        public async Task<string> Run(int time)
        {
            DateTime currentTime = System.DateTime.Now;
            Guid id = Guid.NewGuid();
            Console.WriteLine(id.ToString() + ":Start." + Convert.ToString(currentTime));
  
            var task = new MemoryWorkload();
            await task.Run(time);

            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));

            return $"OK. Memory job done. ";
        }


    }
}
