using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading;
using Saasi.Shared.Workload;

namespace Saasi.Microservices.Combined.Controllers
{
    [Route("api/")]
    public class HomeController : Controller
    {
        // GET api/cpu
        [HttpGet("cpu")]
        public async Task<JsonResult> RunCpu(int time)
        {
            DateTime currentTime = System.DateTime.Now;
            Guid id = Guid.NewGuid();
            Console.WriteLine(id + ":Start." + Convert.ToString(currentTime));
            var task = new CpuWorkload();
            var result = await task.Run(time);
            Console.WriteLine($"{id} :Done. {Convert.ToString(System.DateTime.Now)}");
            return new JsonResult(
                new {
                    Status = "OK",
                    CPULoad = result
                }
            );
        }

        // HTTP GET api/io?read=xxx
        [HttpGet("io")]
        public async Task<JsonResult> RunIo(int read)
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

        // HTTP GET api/memory
        [HttpGet("memory")]
        public async Task<JsonResult> RunMemory(int round)
        {
            DateTime currentTime = System.DateTime.Now;
            Guid id = Guid.NewGuid();
            Console.WriteLine(id.ToString() + ":Start." + Convert.ToString(currentTime));
  
            var task = new MemoryWorkload();
            var result = await task.Run(round);

            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));

            return new JsonResult(
                new {
                    Status = "OK",
                    MemoryWorkload = result
                }
            );
        }
    }
}
