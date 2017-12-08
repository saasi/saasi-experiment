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
        public BusinessMicroservice(DockerClient dockerClient) : base(ContainerType.BusinessMicroservice, dockerClient)
        {
             
        }

        public override async Task DoScale()
        {
            await base.DoScale();
            //Do nothing
        
        }

        public override void CheckResourceUtilisation()
        {
            //foreach (var pair in Containers)
            //{
            //    var container = pair.Value;
            //    if (container.MemoryUsage > MemoryViolationThreshold)
            //    {

            //        MemoryViolationCounter++;
            //        Console.WriteLine($"Memory violation: {container.Id} Total {MemoryViolationCounter}");
            //    }
            //}

            //if (MemoryViolationCounter >= 3 * ActualScale)
            //{
            //    if (LastScaleTime.AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
            //    {
            //        LastScaleTime = DateTime.Now;
            //        ScaleTarget++;
            //        Console.WriteLine($"Memory -> {ScaleTarget}");
            //        WriteScaleOutRecord();
            //        DoScale();
            //    }
            //    MemoryViolationCounter = 0;
            //}
        }


    }
}
