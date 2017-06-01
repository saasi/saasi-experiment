using System;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoadGenerator.MockUsers {
    public class Application3User : BaseApplicationUser
    {
        private string[] _config = new string[] {
            //io cpu memory timetorun timeout
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //5
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //10
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //15
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //20
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //25
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11",
            "1 0 0 10 11", //30            
        };
        public Application3User()
        {
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            _guid = System.Guid.NewGuid().ToString();
        }
        public override async Task Run(string baseURL, int duration)
        {
            var currentTime = System.DateTime.Now;
            var finishTime = currentTime.AddSeconds(duration);
            Console.WriteLine($"User {_guid} ");
            while ( System.DateTime.Now.CompareTo(finishTime) < 0 ){
                // keep looping
                Console.WriteLine($"User {_guid} starts");
                for (int i = 0; i< _config.Length; ++i) {
                    Console.WriteLine($"User {_guid} hitting service {i}");
                    var order = _config[i].Split(' ');
                    var parameters = new Dictionary<string,string>{
                         {"io", order[0]},
                         {"cpu", order[1]},
                         {"memory", order[2]},
                         {"timetorun", order[3]},
                         {"timeout", order[4]},
                         {"guid", _guid},
                         {"businessid", i.ToString()},
                         {"timestart", ((DateTimeOffset)System.DateTime.Now).ToUnixTimeSeconds().ToString()}
                     };
                    var url = new Uri(QueryHelpers.AddQueryString(baseURL+"/Business", parameters));
                    Console.WriteLine(url.ToString());
                    try {
                        var response = await _httpClient.GetAsync(url);
                        Console.WriteLine($"User {_guid} {response.StatusCode}");
                    } catch {
                        Console.WriteLine($"User {_guid} Network Error");
                    }

                }
            }
        }

        
    }
}