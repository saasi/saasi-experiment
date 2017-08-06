using Docker.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor
{
    public class ServiceContainer
    {
        public string Id { get; set; }
        public ContainerType Type { get; set; } = ContainerType.Unknown;
        public double CPUUsage { get; set; } = 0.0;
        public double MemoryUsage { get; set; } = 0.0;
        public double IOUsage { get; set; } = 0.0;  // MB per sec
        private double lastBlockIOTotal = 0.0;
        private DateTime lastBlockIORecordedTime;
        private Timer checkStatsTimer;
        private Timer checkIOStatsTimer;

        private readonly DockerClient _dockerClient;
        private readonly CAdvisorClient _cadvisorClient;
        public ServiceContainer(string id, ContainerType type, DockerClient dockerClient)
        {
            Id = id;
            Type = type;
            _dockerClient = dockerClient;
            _cadvisorClient = new CAdvisorClient(Id);
            lastBlockIORecordedTime = DateTime.Now;

            //  
            checkStatsTimer = new Timer( (object o) =>
            {
                CPUUsage = _cadvisorClient.CPUPercentage;
                MemoryUsage = _cadvisorClient.MemoryPercentage;
                IOUsage = _cadvisorClient.IOMBps;
                LogUsage();
                if (this.Type == ContainerType.CPUMicroservice)
                    Console.WriteLine($"CPU {Id}:{CPUUsage}");
                if (this.Type == ContainerType.IOMicroservice)
                    Console.WriteLine($"IO {Id}:{IOUsage}");
                if (this.Type == ContainerType.MemoryMicroservice)
                    Console.WriteLine($"MEM {Id} :{MemoryUsage}");
                if (this.Type == ContainerType.BusinessFunction)
                    Console.WriteLine($"{DateTime.Now.ToString()} CPU {CPUUsage} IO {IOUsage} MEM {MemoryUsage}");
            }, null, 0, 3000);
        }

        ~ServiceContainer()
        {
            Console.WriteLine($"Container {Id} Died");
            checkStatsTimer.Dispose();
            checkIOStatsTimer.Dispose();
        }

        


        private void LogUsage()
        {
            //Console.WriteLine(this.Type.ToString() + ":" + this.Id + ":" + CPUUsage.ToString());
            if (this.Type == ContainerType.CPUMicroservice)
            {
                StreamWriter sw = File.AppendText("data/cpuStats.txt");
                sw.WriteLine($"{Id} {CPUUsage}");
                sw.Flush();
                sw.Dispose();
            }
            if (this.Type == ContainerType.IOMicroservice)
            {
                StreamWriter sw = File.AppendText("data/ioStats.txt");
                sw.WriteLine($"{Id} {IOUsage}");
                sw.Flush();
                sw.Dispose();
            }
            if (this.Type == ContainerType.MemoryMicroservice)
            {
                StreamWriter sw = File.AppendText("data/memStats.txt");
                sw.WriteLine($"{Id} {MemoryUsage}");
                sw.Flush();
                sw.Dispose();
            }
            if (this.Type == ContainerType.BusinessFunction)
            {
                StreamWriter sw = File.AppendText("data/bmsStats.txt");
                sw.WriteLine($"{Id} {DateTime.Now.ToString()}  CPU:{CPUUsage} IO:{IOUsage} MEM: {MemoryUsage}");
                sw.Flush();
                sw.Dispose();
            }
        }

    }
}
