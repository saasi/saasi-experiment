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
        private static long cellSize = 1024L * 1024L;
        String generateRandomFile = WriteFiles.GenerateRandomStringFile(10*cellSize);

        // HTTP GET api/io?time=xxx
        [HttpGet("io")]

        public async Task<string> Run(int read)
        {
            var r = new Random(read);
            Int64 startByte = r.Next(10, 100000000);
            Int64 length = read * cellSize;
            //DateTime currentTime = System.DateTime.Now;
            //Guid id = Guid.NewGuid();
            Console.WriteLine("IO Microsevices: is running.");
  
            var task = new IoWorkload();
            ExecutionResult result = await task.Run(startByte,length);

            Console.WriteLine("IO Microsevices:Done." );

            return result.ReadResult ;
        }

    }
}
