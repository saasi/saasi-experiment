using System;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Text;

namespace LoadGenerator.MockUsers {
    public class Application3User : BaseApplicationUser
    {
        private string[] _config = new string[] {
            // request param configuration
            //io cpu memory timetorun timeout
            "0 1 0 5 10",
            "1 0 0 5 10",
            "0 0 1 5 10",
            "1 1 0 5 10",
            "1 0 1 5 10",
            "0 1 1 5 10",
            "0 1 0 10 30",
            "0 0 1 10 30",
            "1 1 0 10 30",
            "1 0 1 10 30",
            "0 1 1 10 30",
            "0 1 0 15 60",
            "1 0 0 15 60",
            "0 0 1 15 60",
            "1 1 0 15 60",
            "1 0 1 15 60",
            "0 1 1 15 60",
            "0 1 0 20 80",
            "1 0 0 20 80",
            "0 0 1 20 80",
            "1 1 0 20 80",
            "1 0 1 20 80",
            "0 1 1 20 80",
            "0 1 0 30 120",
            "1 0 0 30 120",
            "0 0 1 30 120",
            "1 1 0 30 120",
            "1 0 1 30 120",
            "0 1 1 30 120",

        };

        public Application3User()
        {
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            _guid = System.Guid.NewGuid().ToString();

        }
        public override async Task Run(string baseURL)
        {
            Console.WriteLine($"User {_guid} ");
                for (int i = 0; i< _config.Length; ++i) {

                        var order = _config[i].Split(' ');
                        var timestart = System.DateTime.Now;
                        var parameters = new Dictionary<string, string>{
                                    {"io", order[0]},
                                    {"cpu", order[1]},
                                    {"memory", order[2]},
                                    {"timetorun", order[3]},
                                    {"timeout", order[4]},
                                    {"guid", _guid},
                                    {"businessid", i.ToString()},
                                    {"timestart", ((DateTimeOffset)timestart).ToUnixTimeSeconds().ToString()}
                                };
                        var url = new Uri(QueryHelpers.AddQueryString(baseURL, parameters));
                        //var url = new Uri(QueryHelpers.AddQueryString("http://10.137.0.82:5001/saasi" + "/Business", parameters));

                        try
                        {
                            var response =await _httpClient.GetAsync(url);
                            Console.WriteLine($"User {_guid} {response.StatusCode} {DateTime.Now.ToString()} {timestart}");

                    }
                        catch
                        {
                            Console.WriteLine($"User {_guid} Network Error");
                        }
;
		        }

        }

        
    }
}
