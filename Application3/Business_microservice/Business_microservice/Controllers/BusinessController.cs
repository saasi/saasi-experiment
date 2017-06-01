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
        public IActionResult Index()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            using (var channel2 = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "fanout");
                string message = ConfigSettings.order[0];
                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange: "call",
                                      routingKey: "",
                                      basicProperties: properties,
                                      body: body);
                return View();
            }
        }
    }
}
