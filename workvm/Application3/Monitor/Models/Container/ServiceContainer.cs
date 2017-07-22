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
            }, null, 0, 3000);
        }

        ~ServiceContainer()
        {
            Console.WriteLine($"Container {Id} Died");
            checkStatsTimer.Dispose();
            checkIOStatsTimer.Dispose();
        }

        

        public async Task UpdateIOUsageAsync()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./stats.sh " + this.Id, };
            Process ip = new Process() { StartInfo = startInfo };
            try
            {
                ip.Start();
                Thread.Sleep(3000);
                var lines = File.ReadAllLines(@"stats.txt");

                List<string> list = new List<string>();
                foreach (string s in lines)
                {
                    if (s.Contains(this.Id))
                    {
                        list.Add(s);
                    }
                }
                var line = list.ToArray()[0].Split(' ');

                List<string> list2 = new List<string>();
                foreach (string s in line)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        list2.Add(s);
                    }
                }
                var data = list2.ToArray();

              
                Console.WriteLine($"## CPU ds { Convert.ToDouble(data[1].Substring(0, data[1].Length - 1)) } vs. ca {CPUUsage}");

                //
                Console.WriteLine($"## MEM ds { Convert.ToDouble(data[7].Substring(0, data[7].Length - 1)) } vs. ca {MemoryUsage}");
                {

                    double newBlockIOTotal = lastBlockIOTotal;
                    if (data[17].Equals("MB"))
                    {
                        newBlockIOTotal = Convert.ToDouble(data[16]);
                    }
                    else if (data[17].Equals("GB"))
                    {
                        newBlockIOTotal = Convert.ToDouble(data[16]) * 1024;
                    }
                        

                    Console.WriteLine(newBlockIOTotal.ToString());
                    DateTime now = DateTime.Now;
                    TimeSpan timeElapsed = now- lastBlockIORecordedTime;
                    
                    IOUsage = (newBlockIOTotal - lastBlockIOTotal) / timeElapsed.TotalSeconds;
                    Console.WriteLine($"LastIO {lastBlockIOTotal} at {lastBlockIORecordedTime}, NewIO {newBlockIOTotal} at {now}, speed = {IOUsage} MB/s");
                    lastBlockIORecordedTime = now;
                    lastBlockIOTotal = newBlockIOTotal;
                }
            }
            catch
            {
                Console.WriteLine($"Error reading stats for container {Id} of {Type.ToString()}.");
            }
            finally
            {
                // Must kill the docker stats process. Otherwise CPU will be used up.
                try
                {
                    ip.Kill();
                    ip.Dispose();
                }
                catch
                {
                    //do nothing
                }

            }

            
        }

        private void LogUsage()
        {
            //Console.WriteLine(this.Type.ToString() + ":" + this.Id + ":" + CPUUsage.ToString());
            if (this.Type == ContainerType.CPUMicroservice)
            {
                StreamWriter sw = File.AppendText("data/cpuStats.txt");
                sw.WriteLine($"{Convert.ToString(System.DateTime.Now)} {Id} CPU={CPUUsage}");
                sw.Flush();
                sw.Dispose();
            }
            if (this.Type == ContainerType.IOMicroservice)
            {
                StreamWriter sw = File.AppendText("data/ioStats.txt");
                sw.WriteLine($"{Convert.ToString(System.DateTime.Now)} {Id} IO={IOUsage}");
                sw.Flush();
                sw.Dispose();
            }
            if (this.Type == ContainerType.MemoryMicroservice)
            {
                StreamWriter sw = File.AppendText("data/memStats.txt");
                sw.WriteLine($"{Convert.ToString(System.DateTime.Now)} {Id} Memory={MemoryUsage}");
                sw.Flush();
                sw.Dispose();
            }
        }

    }
}
