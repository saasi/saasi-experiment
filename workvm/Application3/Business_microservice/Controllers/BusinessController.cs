using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Business_microservice.Controllers
{
    public class BusinessController : Controller
    {
        private Configuration ConfigSettings { get; set; }
        // GET: /<controller>/
        public BusinessController(IOptions<Configuration> settings)
        {
            ConfigSettings = settings.Value;
        }
        //public IActionResult Index(Guid guid, DateTime timestart, int? io = 0, int? cpu = 0, int? memory = 0, int timetorun = 0, int id = 0, int timeout = 0)
        public IActionResult Index(Guid guid, Guid bmsguid, int? io = 0, int? cpu = 0, int? memory = 0, int timetorun = 0, int id = 0, int timeout = 0)
        {
            
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            
         //   using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
//channel.ExchangeDeclare(exchange: "mono", type: "direct");
                string message = Convert.ToString(io) + " " + Convert.ToString(cpu) + " " + Convert.ToString(memory) + " " + Convert.ToString(timetorun) + " " +  Convert.ToString(timeout);
                Console.WriteLine(message);
            //  var body = Encoding.UTF8.GetBytes(message);
            // var properties = channel.CreateBasicProperties();
            //  properties.Persistent = true;
            //  channel.BasicPublish(exchange: "mono",
            //                       routingKey: "business",
            //                        basicProperties: properties,
            //                        body: body);



            return new JsonResult(new Dictionary<string, string> { { "status", "ok" } });
           // }
        }


    }
}

