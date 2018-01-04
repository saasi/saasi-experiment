using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Saasi.Microservices.Cpu
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8088")
                .UseStartup<Startup>()
                .UseShutdownTimeout(new TimeSpan(0,5,0)) // 5 minutes shutdown time
                .UseApplicationInsights()
                .Build();
    }
}
