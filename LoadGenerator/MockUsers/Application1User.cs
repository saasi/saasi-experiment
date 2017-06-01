using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoadGenerator.MockUsers {
    public class Application1User : BaseApplicationUser
    {
        
        public override async Task Run(string baseURL, int duration)
        {
            var currentTime = System.DateTime.Now;
            var finishTime = currentTime.AddSeconds(duration);
            Console.WriteLine($"User {_guid} ");
            while ( System.DateTime.Now.CompareTo(finishTime) < 0 ){
                // keep looping
                Console.WriteLine($"User {_guid} requesting");
    
                var url = new Uri(baseURL+"/Business");
                Console.WriteLine(url.ToString());
                try {
                    var response = await _httpClient.GetAsync(url);
                    Console.WriteLine($"User {_guid} {response.StatusCode}");
                } catch {
                    Console.WriteLine($"User {_guid} Network Error");
                }
                Thread.Sleep(500);
            }           
        }
    }
}