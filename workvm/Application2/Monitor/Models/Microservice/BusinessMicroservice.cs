using System;
using System.Collections.Generic;
using System.Text;
using Docker.DotNet;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Monitor
{
    public class BusinessMicroservice : Microservice
    {
        private static double CPUViolationThresdhold = 80.0;
        private int CPUViolationCounter = 0;
        private static double MemoryViolationThreshold = 40.0;
        private int MemoryViolationCounter = 0;
        private static double IOViolationThresdhold = 30.0;
        private int IOViolationCounter = 0;
        public BusinessMicroservice(DockerClient dockerClient) : base(ContainerType.BusinessFunction, dockerClient)
        {
             
        }

        public override async Task DoScale()
        {
            await base.DoScale();
            Console.WriteLine("scaleout bms");
            ProcessStartInfo statInfo1 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./scalebms1.sh " + ScaleTarget }; //Again, scriptfile should be in working directory
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
                if (container.IOUsage > IOViolationThresdhold && container.IOUsage < 50)
                {
                    IOViolationCounter++;
                    Console.WriteLine($"IO violation: {container.Id} Total {IOViolationCounter}");
                }
                if (container.MemoryUsage > MemoryViolationThreshold)
                {
                    MemoryViolationCounter++;
                    Console.WriteLine($"MEMORY violation: {container.Id} Total {MemoryViolationCounter}");
                }
            }

            if ((CPUViolationCounter >= 3))
            {
                if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0)
                {
                    LastScaleTime = DateTime.Now;
                    ScaleTarget++;
                    Console.WriteLine($"BMS -> {ScaleTarget}");
                    WriteScaleOutRecord();
                    DoScale();
                    CPUViolationCounter = 0;

                    return;
                }

            }

            if (MemoryViolationCounter >= 3)
            {
                if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0)
                {
                    LastScaleTime = DateTime.Now;
                    ScaleTarget++;
                    Console.WriteLine($"BMS -> {ScaleTarget}");
                    WriteScaleOutRecord();
                    DoScale();
                    MemoryViolationCounter = 0;
                    return;


                }


            }

            if (IOViolationCounter >= 3)
            {
                if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0)
                {
                    LastScaleTime = DateTime.Now;
                    ScaleTarget++;
                    Console.WriteLine($"BMS -> {ScaleTarget}");
                    WriteScaleOutRecord();
                    DoScale();
                    IOViolationCounter = 0;

                    return;
                }

            }
        }


    }
}
