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
        // HTTP GET api/io?read=xxx
        [HttpGet("io")]
        public async Task<JsonResult> Run(int read)
        {
            var r = new Random();
            Int64 startByte = ((long)r.Next(10, 100000000) * (long)r.Next(10, 100000000)) % (Program.cellSize*(Program.cellCount-1L));
            Int64 length = read * Program.cellSize;
            //DateTime currentTime = System.DateTime.Now;
            //Guid id = Guid.NewGuid();
            Console.WriteLine("IO Microsevices: is running.");
  
            var task = new IoWorkload();
            ExecutionResult result = await task.Run(startByte, length);

            Console.WriteLine("IO Microsevices:Done." );

            return new JsonResult(
                new {
                    Status = "OK",
                    IOLoad = result
                }
            ); 
        }

    }
}
