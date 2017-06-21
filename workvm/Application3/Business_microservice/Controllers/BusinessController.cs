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


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public IActionResult Index(Guid guid, DateTime timestart, int? io = 0, int? cpu = 0, int? memory = 0, int timetorun = 0, int id = 0, int timeout = 0)
        {
            DateTime startRunTime = System.DateTime.Now;
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "fanout");
                string message = Convert.ToString(io) + " " + Convert.ToString(cpu) + " " + Convert.ToString(memory) + " " + Convert.ToString(timetorun);
                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange: "call",
                                      routingKey: "",
                                      basicProperties: properties,
                                      body: body);
                DateTime currentTime = new DateTime();
                currentTime = System.DateTime.Now;
                DateTime finishTime = currentTime.AddSeconds(timetorun);
                while (System.DateTime.Now.CompareTo(finishTime) < 0)
                {
                    GenerateRandomString(100);
                }
                DateTime completeRunTime = System.DateTime.Now;
                if (startRunTime.AddSeconds(timeout).CompareTo(completeRunTime) > 0) //check timeout
                    reportDM(id, guid);
                return View();
            }
        }
        private static string GenerateRandomString(int length)
        {
            var r = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int c = r.Next(97, 123);
                sb.Append(Char.ConvertFromUtf32(c));
            }
            return sb.ToString();
        }

        private static void reportDM(int id, Guid guid) //send message to DM
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "report_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                var message = Convert.ToString(id) + " " + guid;
                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "report_queue",
                                     basicProperties: properties,
                                     body: body);

            }
        }
    }
}

