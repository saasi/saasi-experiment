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

        private readonly DockerClient _dockerClient;
        public ServiceContainer(string id, ContainerType type, DockerClient dockerClient)
        {
            Id = id;
            Type = type;
            _dockerClient = dockerClient;
            lastBlockIORecordedTime = DateTime.Now;

            checkStatsTimer = new Timer(async (object o) => { await UpdateUsageAsync(); }, null, 0, 500);
        }

        ~ServiceContainer()
        {
            checkStatsTimer.Dispose();
        }


        private async Task<double> GetCPUUsage()
        {
            //_dockerClient.Containers.GetContainerStatsAsync(this.Id);

        }
        public async Task UpdateUsageAsync()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./stats.sh " + this.Id, };
            Process ip = new Process() { StartInfo = startInfo };
            try
            {
                ip.Start();
                Thread.Sleep(500);
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

                CPUUsage = Convert.ToDouble(data[1].Substring(0, data[1].Length - 1));
     
                MemoryUsage = Convert.ToDouble(data[7].Substring(0, data[7].Length - 1));

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
            Console.WriteLine(this.Type.ToString() + ":" + this.Id + ":" + CPUUsage.ToString());
            StreamWriter sw = File.AppendText("data/apiStats.txt");
            sw.WriteLine($"{Convert.ToString(System.DateTime.Now)} {this.Type.ToString()} {Id} CPU={CPUUsage} Memory={MemoryUsage} IO={IOUsage}");
            sw.Flush();
            sw.Dispose();
        }

    }
}
