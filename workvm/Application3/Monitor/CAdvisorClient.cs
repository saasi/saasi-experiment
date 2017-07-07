using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
    public class CAdvisorClient
    {

        public string EndPoint { get; private set; } = "";

        private readonly HttpClient _httpClient;
        public CAdvisorClient(string endpoint)
        {
            EndPoint = endpoint;
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
        }

        public async Task<double> GetContainerCPUUsageAsync(string id)
        {
            var jsonResponse = await HTTPGetAsync($"{EndPoint}/api/v1.3/docker/{id}");
            dynamic stats = JObject.Parse(jsonResponse);
            
            dynamic tmp = stats[$"/system.slice/docker-{id}.scope"];
            JArray statsArray = tmp["stats"];

            if (statsArray.Count == 0)
            {
                throw new Exception("No stats data");
            }
            dynamic cur = statsArray[statsArray.Count - 1];
            dynamic prev = statsArray[statsArray.Count - 2];
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
            return cpuPercentage;

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
