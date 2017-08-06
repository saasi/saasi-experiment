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
                Console.WriteLine($"User {_guid} requesting");
                var timestart = DateTime.Now;

                var url = new Uri("http://10.137.0.81:5000/saasi/Business?timestart=" + ((DateTimeOffset)timestart).ToUnixTimeSeconds().ToString());
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