using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Saasi.Shared.Workload;

namespace Saasi.Microservices.Io
{
    public class Program
    {        
        public static long cellSize = 1024L * 1024L;
        public static long cellCount = 10L;
        public static void Main(string[] args)
        {
            Console.WriteLine(IoWorkload.GenerateRandomStringFile(cellSize*cellCount));
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:80")
                .UseStartup<Startup>()
                .UseShutdownTimeout(new TimeSpan(0,5,0)) // 5 minutes shutdown time
                .Build();
    }
}
