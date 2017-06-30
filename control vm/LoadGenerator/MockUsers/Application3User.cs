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
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",
            "1 1 1 10 20",


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
        public override async Task Run(string baseURL, int duration)
        {
            var currentTime = System.DateTime.Now;
            var finishTime = currentTime.AddSeconds(duration);
            Console.WriteLine($"User {_guid} ");
            //while ( System.DateTime.Now.CompareTo(finishTime) < 0 ){
                // keep looping
            for (int j = 0; j < duration; j++)
            { 
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
                    //var url = new Uri(QueryHelpers.AddQueryString(baseURL+"/Business", parameters));
                    var url = new Uri(QueryHelpers.AddQueryString("http://192.168.99.100:5001" + "/Business", parameters));
                        using (connection = factory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            channel.ExchangeDeclare(exchange: "url", type: "direct");
                            var body = Encoding.UTF8.GetBytes(url.ToString().Split('/')[3]);
                       // var properties = channel.CreateBasicProperties();
                      //  properties.Persistent = true;
                            channel.BasicPublish(exchange: "url",
                                              routingKey: "dispatch",
                                              basicProperties: null,
                                              body: body);
                            Console.WriteLine(url.ToString().Split('/')[3]);
                        }
                        //System.Threading.Thread.Sleep(1000);*/
                   /* try {
                          var response = await _httpClient.GetAsync(url);
                          Console.WriteLine($"User {_guid} {response.StatusCode}");
                      } catch {
                          Console.WriteLine($"User {_guid} Network Error");
                      }
                }*/
            }

        }

        
    }
}
