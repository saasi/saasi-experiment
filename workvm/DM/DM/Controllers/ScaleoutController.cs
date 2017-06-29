using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DM.Controllers
{
    public class ScaleoutController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(Guid bmsguid)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "dm", type: "direct");
                var message = bmsguid.ToString();
                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange: "dm",
                                      routingKey: "scaleout",
                                      basicProperties: properties,
                                      body: body);
                Console.WriteLine("scaleout:" + bmsguid);
                return View();
            }
        }
    }
}
