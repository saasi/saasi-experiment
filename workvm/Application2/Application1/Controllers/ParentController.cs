using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using RabbitMQ.Client;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Application2.Controllers
{
    public class ParentController : Controller
    {
        private Configuration ConfigSettings { get; set; }
        // GET: /<controller>/
        public ParentController(IOptions<Configuration> settings)
        {
            ConfigSettings = settings.Value;
        }
        // GET: /<controller>/
        public IActionResult Index(String timestart)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };

            for (int i = 0; i < 30; i++)
            {
                var order = ConfigSettings.record[i].Split(' ');
                int io = Convert.ToInt16(order[0]);
                int cpu = Convert.ToInt16(order[1]);
                int memory = Convert.ToInt16(order[2]);
                int timetorun = Convert.ToInt16(order[3]);
                int timeout = Convert.ToInt16(order[4]);
                // Business business = new Business(order);
                //new Thread(business.Fun).Start();
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "mono", type: "direct");
                    string message = Convert.ToString(io) + " " + Convert.ToString(cpu) + " " + Convert.ToString(memory) + " " + Convert.ToString(timetorun) + " " + Convert.ToString(timeout) + " " + timestart;
                    Console.WriteLine(message);
                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish(exchange: "mono",
                                          routingKey: "business",
                                          basicProperties: properties,
                                          body: body);


                    //   DateTime completeRunTime = System.DateTime.Now;
                    // if (startRunTime.AddSeconds(timeout).CompareTo(completeRunTime) > 0) //check timeout
                    /*    Console.WriteLine("send to DM");
                        var message2 = Convert.ToString(id) + " " + guid;
                        var body2 = Encoding.UTF8.GetBytes(message2);
                        channel.BasicPublish(exchange: "call",
                              routingKey: "report",
                              basicProperties: null,
                              body: body2);
                              */
                }
            }
            
                
            return View();
        }
    }
}
