using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using Docker.DotNet;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Monitor
{
    class Monitor
    {
        private static DockerClient dockerClient;
        private static string vmaddress;
        private static IOMicroservice ioMicroservice;
        private static MemoryMicroservice memoryMicroservice;
        private static CPUMicroservice cpuMicroservice;
        private static Dictionary<string, int> bms = new Dictionary<string, int>();
        private static Dictionary<string, DateTime> scaleTime = new Dictionary<string, DateTime>();
        private static int bmsNum = 1;

        public static void Main(string[] args)
        {
            try
            {
                dockerClient = new DockerClientConfiguration(new Uri("http://127.0.0.1:4243"))
                                .CreateClient();
                // dockerClient = new DockerClientConfiguration(new Uri("http://192.168.0.1:4243"))
                //  .CreateClient();
            }
            catch
            {
                Console.WriteLine("Could not connect to Docker Remote API.");
                return;
            }



            ioMicroservice = new IOMicroservice(dockerClient); // monitor io_microservice
            cpuMicroservice = new CPUMicroservice(dockerClient);// monitor cpu_microservice
            memoryMicroservice = new MemoryMicroservice(dockerClient);// monitor memory_microservice

            while (true)
            {
                Thread.Sleep(5000);
                Console.WriteLine($"##########{DateTime.Now}##########");
                Console.WriteLine($"CPU {cpuMicroservice.ActualScale}->{cpuMicroservice.ScaleTarget}");
                Console.WriteLine($"IO {ioMicroservice.ActualScale}->{ioMicroservice.ScaleTarget}");
                Console.WriteLine($"Memory {memoryMicroservice.ActualScale}->{memoryMicroservice.ScaleTarget}");
                Console.WriteLine("###########################################");
            }

        }

        public Monitor()
        {
          
        }
    }
}
