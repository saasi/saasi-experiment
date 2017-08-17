using System;
using System.Collections.Generic;
using System.Text;
using Docker.DotNet;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Monitor
{
    public class IOMicroservice : Microservice
    {
        private static double IOViolationThresdhold = 30.0;
        private int IOViolationCounter = 0;
        public IOMicroservice(DockerClient dockerClient) : base(ContainerType.IOMicroservice, dockerClient)
        {
             
        }

        public override async Task DoScale()
        {
            await base.DoScale();
            Console.WriteLine("scaleout io");
            ProcessStartInfo statInfo1 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./scaleio1.sh " + this.ScaleTarget }; 
            Process stat = new Process() { StartInfo = statInfo1, };
            stat.Start();
        }

        public override void CheckResourceUtilisation()
        {
            foreach (var pair in Containers)
            {
                var container = pair.Value;
                if (container.IOUsage > IOViolationThresdhold && container.IOUsage < 50)
                {

                    IOViolationCounter++;
                    Console.WriteLine($"IO violation: {container.Id} Total {IOViolationCounter}");
                }
            }

            if (IOViolationCounter >= 5 * ActualScale)
            {
                if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
                {
                    LastScaleTime = DateTime.Now;
                    ScaleTarget++;
                    Console.WriteLine($"IO -> {ScaleTarget}");
                    WriteScaleOutRecord();
                    DoScale();
                }
                IOViolationCounter = 0;
            }
        }


    }
}
