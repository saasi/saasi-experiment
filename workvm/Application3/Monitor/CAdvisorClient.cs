using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor
{
    public class CAdvisorClient
    {
        public static readonly string DefaultEndpoint = "http://localhost:8080";
        public string EndPoint { get; private set; } = "";
        private string ContainerId { get; set; } = "";
        public double CPUPercentage { get; private set; } = 0.0;
        public double MemoryPercentage { get; private set; } = 0.0;
        
        public double IOMBps {get; private set;} = 0.0;
        private long? memoryLimit = null;
        private readonly Timer _statsTimer;

        private readonly HttpClient _httpClient;

        private bool _requestOnGoing = false;
        public CAdvisorClient(string id)
        {
            EndPoint = DefaultEndpoint;
            ContainerId = id;
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;

            _statsTimer = new Timer(async (object o) => { 
                
                if (!_requestOnGoing) {
                    _requestOnGoing = true;
                    try
                    {
                        await UpdateStatsAsync();
                    }
                    catch (Exception e)
                    {
                        Console.Write("Exception in CAdvisorClient: ");
                        Console.WriteLine($"Message={e.Message} Source={e.Source} Trace={e.StackTrace}");
                    }
                    _requestOnGoing = false;
                }
             
            }, null, 0, 3000);

        }

        ~CAdvisorClient()
        {
            _statsTimer.Dispose();
        }

        public async Task UpdateStatsAsync()
        {
            //Console.WriteLine($"=================={ContainerId}");
            var jsonResponse = await HTTPGetAsync($"{EndPoint}/api/v1.3/docker/{ContainerId}");


            dynamic result = JObject.Parse(jsonResponse);
            var properties = new List<String>();
            foreach (JProperty jp in result) {
                //Console.WriteLine(jp.Name);
                properties.Add(jp.Name);
            }

            if (properties.Count == 0) {
                Console.WriteLine("No properties");
            }

            dynamic tmp = result[properties[0]];//$"/system.slice/docker-{ContainerId}.scope"];

            if (tmp == null) return;

            if (memoryLimit == null)
            {
                long containerMemoryLimit = tmp.spec.memory.limit;
                var machineMemLimit = await GetMachineMemoryCapacityAsync();
                this.memoryLimit = Math.Min(containerMemoryLimit, machineMemLimit);
               // Console.WriteLine($"Memory Limit {memoryLimit/1024.0/1024.0} MB");
            }

            CPUPercentage = await GetContainerCPUUsageAsync(tmp.stats);
            MemoryPercentage = await GetContainerMemoryUsageAsync(tmp.stats);
            IOMBps = await GetBlockIOAsync(tmp.stats);
            //Console.WriteLine("Updated cadvisor");
        }


        private async Task<long> GetMachineMemoryCapacityAsync()
        {
            var jsonResponse = await HTTPGetAsync($"{EndPoint}/api/v1.3/machine");
            dynamic result = JObject.Parse(jsonResponse);
            long memoryCapacity = result.memory_capacity;
            return memoryCapacity;
        }

        private async Task<double> GetContainerCPUUsageAsync(JArray statsArray)
        {
            if (statsArray.Count <= 10)
            {
                Console.WriteLine("Not enough stats data");
                throw new Exception("Not enough stats data");
            }
            dynamic cur = statsArray[statsArray.Count - 1];
            dynamic prev = statsArray[statsArray.Count - 9];
            string curCPU = cur.cpu.usage.total;
            string prevCPU = prev.cpu.usage.total;
            Int64 curCPUUsage = 0; 
            Int64 prevCPUUsage = 0;
            Int64.TryParse(curCPU, out curCPUUsage);
            Int64.TryParse(prevCPU, out prevCPUUsage);
            DateTime curTime = cur.timestamp;
            DateTime prevTime = prev.timestamp;
            TimeSpan interval = curTime - prevTime;
            double intervalNs = interval.TotalMilliseconds * 1000000; // ms -> ns
            double cpuPercentage = 100.0 * (double)(curCPUUsage - prevCPUUsage) / intervalNs ;
            //Console.WriteLine($"prev {prevCPUUsage} cur {curCPUUsage} totaltime {intervalNs}ns, {cpuPercentage}%");
            //Console.WriteLine($"CPU: {cpuPercentage}%");
            return cpuPercentage;
        }

        private async Task<double> GetBlockIOAsync(JArray statsArray)
        {
            if (statsArray.Count <= 10)
            {
                Console.WriteLine("Not enough stats data");
                throw new Exception("Not enough stats data");
            }
            dynamic cur = statsArray[statsArray.Count - 1];
            dynamic prev = statsArray[statsArray.Count - 9];
            string curIO = cur.diskio.io_service_bytes[0].stats.Total;
            string prevIO = prev.diskio.io_service_bytes[0].stats.Total;;
            Int64 curIOUsage = 0; 
            Int64 prevIOUsage = 0;
            Int64.TryParse(curIO, out curIOUsage);
            Int64.TryParse(prevIO, out prevIOUsage);
            DateTime curTime = cur.timestamp;
            DateTime prevTime = prev.timestamp;
            TimeSpan interval = curTime - prevTime;
            double intervalSeconds = interval.TotalSeconds; 
            double IODeltaMB = ((double)curIOUsage - prevIOUsage)/1024.0/1024.0; //MB delta
            double ioMBps =  IODeltaMB/ intervalSeconds;
            // Console.WriteLine($"IOIOOIOIIOIOIO prev {prevIOUsage} cur {curIOUsage} totaltime {intervalSeconds} s, {ioMBps} MB/s");
            //Console.WriteLine($"IO {ioMBps} MB/s");
            return ioMBps;
        }

        private async Task<double> GetContainerMemoryUsageAsync(JArray statsArray)
        {
            if (statsArray.Count == 0)
            {
                throw new Exception("No stats data");
            }
            dynamic cur = statsArray[statsArray.Count - 1];
            
            string curMeory = cur.memory.usage;
            Int64 curMemoryUsage = 0;
            Int64.TryParse(curMeory, out curMemoryUsage);

            //Console.WriteLine($"Memory {curMemoryUsage} bytes {(double)curMemoryUsage /1024.0/1024.0} Limit{(double)memoryLimit/1024.0/1024.0}");
            var totalMemory = (100.0 * curMemoryUsage ) / ((double)(memoryLimit ?? 1));
            //Console.WriteLine($"mem {totalMemory}%");

            return totalMemory;
        }

        private async Task<string> HTTPGetAsync(string url)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync(url))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new HttpRequestException("Not OK.");
                }
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

                    if (result == null || !result.StartsWith("{") || !result.EndsWith("}"))
                    {
                        throw new HttpRequestException("Non-JSON response.");
                    }
                    return result;
                }
            }
        }
    }
}
