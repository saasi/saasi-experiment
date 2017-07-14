using System;
using System.Collections.Generic;
using System.Text;
using Docker.DotNet;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Docker.DotNet.Models;
using System.IO;

namespace Monitor
{
    public class Microservice
    {
        public ContainerType Type { get; set; }
        public DateTime LastScaleTime { get; protected set; }
        private readonly DockerClient _dockerClient;
        public int ScaleTarget { get; protected set; } = 1;
        public ConcurrentDictionary<string, ServiceContainer> Containers { get; set; }
        public int ActualScale { get { return Containers.Count; } }
        private Timer updateTimer;
        private Timer monitorTimer;

        public Microservice(ContainerType type, DockerClient dockerClient)
        {
            Type = type;
            _dockerClient = dockerClient;
            Containers = new ConcurrentDictionary<string, ServiceContainer>();
            this.updateTimer = new Timer(async (object o)=> { await UpdateContainerList(); }, null, 0, 20000);

            this.monitorTimer = new Timer((object o) => { CheckResourceUtilisation(); }, null, 1000, 10000);

            DoScale();
        }

        ~Microservice()
        {
            this.updateTimer.Dispose();
            this.monitorTimer.Dispose();
        }

        // Perform the scale out operation
        public virtual async Task DoScale()
        {
            LastScaleTime = DateTime.Now;
            
        }

        // Make decisions about scaleout
        public virtual void CheckResourceUtilisation()
        {

        }

        public async Task UpdateContainerList()
        {
            var containersList = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
            var newContainers = new ConcurrentDictionary<string, string>();
            foreach (var container in containersList)
            {
                if (container.Image.Equals(ContainerTypeToString(this.Type))) {
                    newContainers.TryAdd(container.ID, "placeholder");
                }
            }
          
            // diff

            // remove stopped containers
            foreach (var pair in this.Containers)
            {
                if (newContainers.ContainsKey(pair.Key))
                {
                    ServiceContainer value;
                    this.Containers.TryRemove(pair.Key, out value);
                }
            }
            // add or update new containers
            foreach (var pair in newContainers)
            {
                if (this.Containers.ContainsKey(pair.Key))
                {
                    //ServiceContainer value;
                    //this.Containers.TryGetValue(pair.Key, out value);
                    //this.Containers.TryUpdate(pair.Key, pair.Value, value);
                } else
                {
                    this.Containers.TryAdd(pair.Key, new ServiceContainer(pair.Key, this.Type, this._dockerClient));
                }
            }

            Console.WriteLine($"[{this.Type.ToString()}] Updated container list. => {this.ActualScale} containers.");
            //this.Containers.Clear();
            //this.Containers = newContainers;
        }

        public ContainerType StringToContainerType(string str)
        {
            if (str.Equals("io_microservice"))
            {
                return ContainerType.IOMicroservice;
            }
            else if (str.Equals("cpu_microservice")) {
                return ContainerType.CPUMicroservice;
            }
            else if (str.Equals("memory_microservice"))
            {
                return ContainerType.MemoryMicroservice;
            }
            else if (str.Equals("business_microservice"))
            {
                return ContainerType.BusinessMicroservice;
            }
            else
            {
                return ContainerType.Unknown;
            }
        }

        public string ContainerTypeToString(ContainerType type)
        {
            switch (type)
            {
                case ContainerType.IOMicroservice:
                    return "io_microservice";
                case ContainerType.CPUMicroservice:
                    return "cpu_microservice";
                case ContainerType.MemoryMicroservice:
                    return "memory_microservice";
                case ContainerType.BusinessFunction:
                    return "BusinessFunction";
                default:
                    return "";
            }
        }

        public virtual void WriteScaleOutRecord()
        {
            StreamWriter sw = File.AppendText("data/api-scaleout.txt");
            sw.WriteLine($"Type={Type.ToString()} ScaleTarget={ScaleTarget} {Convert.ToString(System.DateTime.Now)}");
            sw.Flush();
            sw.Dispose();
        }
    }
}
