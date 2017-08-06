using System;
using System.Collections.Generic;
using System.Text;
using Docker.DotNet;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Monitor
{
    public class CPUMicroservice : Microservice
    {
        private static double CPUViolationThresdhold = 80.0;
        private int CPUViolationCounter = 0;
        public CPUMicroservice(DockerClient dockerClient) : base(ContainerType.CPUMicroservice, dockerClient)
        {
             
        }

        public override async Task DoScale()
        {
            await base.DoScale();
            Console.WriteLine("scaleout cpu");
            ProcessStartInfo statInfo1 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./scalecpu1.sh " + this.ScaleTarget }; 
            Process stat = new Process() { StartInfo = statInfo1, };
            stat.Start();
        }

        public override void CheckResourceUtilisation()
        {
            foreach (var pair in Containers)
            {
                var container = pair.Value;
                if (container.CPUUsage > CPUViolationThresdhold)
                {

                    CPUViolationCounter++;
                    Console.WriteLine($"CPU violation: {container.Id} Total {CPUViolationCounter}");
                }
            }

            if (CPUViolationCounter >= 3 * ActualScale)
            {
                if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
                {
                    LastScaleTime = DateTime.Now;
                    ScaleTarget++;
                    Console.WriteLine($"CPU -> {ScaleTarget}");
                    WriteScaleOutRecord();
                    DoScale();
                }
                CPUViolationCounter = 0;
            }
        }


    }
}
