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
            //io cpu memory timetorun timeout
            "0 1 0 5 10",
            "0 0 0 5 10",
            "0 1 0 5 10",
            "0 0 0 5 10",
            "1 0 0 5 10",
            "0 0 0 5 10",
            "1 0 0 5 10",
            "0 0 0 5 10",
            "0 1 0 5 10",
            "0 1 0 5 10",
            "0 0 0 5 10",
            "1 0 0 5 10",
            "1 0 0 5 10",
            "0 0 0 5 10",
            "0 0 0 5 10",
            "1 0 0 5 10",
            "0 1 0 5 10",
            "0 0 0 5 10",
            "0 1 0 5 10",
            "1 0 0 5 10",
            "1 0 0 5 10",
            "0 1 0 5 10",
            "0 0 0 5 10",
            "0 1 0 5 10",
            "1 0 0 5 10",
            "1 0 0 5 10",
            "0 1 0 5 10",
            "0 1 0 5 10",
            "1 0 0 5 10",
            "0 0 0 5 10",

        };
        private IConnection connection;
        private IModel channel;
        private ConnectionFactory factory;
        public Application3User()
        {
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            _guid = System.Guid.NewGuid().ToString();
            factory = new ConnectionFactory() { HostName = "localhost" };

        }
        public override async Task Run(string baseURL, int requestCount)
        {
            var currentTime = System.DateTime.Now;
            var finishTime = currentTime.AddSeconds(requestCount);
            Console.WriteLine($"User {_guid} ");
            //while ( System.DateTime.Now.CompareTo(finishTime) < 0 ){
                // keep looping
            for (int j = 0; j < requestCount; j++)
            { 
                Console.WriteLine($"User {_guid} request #{j} of {requestCount}");
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
                        //var url = new Uri(QueryHelpers.AddQueryString(baseURL+"/Business", parameters));
                        var url = new Uri(QueryHelpers.AddQueryString("http://10.137.0.81:5000" + "/Business", parameters));
                        /*        channel.ExchangeDeclare(exchange: "url", type: "direct");
                                var body = Encoding.UTF8.GetBytes(url.ToString().Split('/')[3]);
                        // var properties = channel.CreateBasicProperties();
                        //  properties.Persistent = true;
                                channel.BasicPublish(exchange: "url",
                                                routingKey: "dispatch",
                                                basicProperties: null,
                                                body: body);
                                Console.WriteLine($"User {_guid} hitting service {i} " + DateTime.Now.ToString() +" " + timestart.ToString());
                            }*/
                        try
                        {
                            var response =await _httpClient.GetAsync(url);
                            Console.WriteLine($"User {_guid} {response.StatusCode} {DateTime.Now.ToString()} {timestart}");
                        //Console.WriteLine($"User {_guid} {DateTime.Now.ToString()} {timestart}");
                    }
                        catch
                        {
                            Console.WriteLine($"User {_guid} Network Error");
                        }
                    
		        }
                System.Threading.Thread.Sleep(20000);
            }
        }

        
    }
}
