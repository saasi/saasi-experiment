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
            //Console.WriteLine(stats.ToString());
            Console.WriteLine(stats[0].ToString());
            return 0;

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
