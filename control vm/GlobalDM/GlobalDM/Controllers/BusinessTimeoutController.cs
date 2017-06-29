using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalDM.Controllers
{
    public class BusinessTimeoutController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(Guid bmsguid)
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            Console.WriteLine(ip.ToString());
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //send container information to dispatch
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "global", type: "direct");
                var body = Encoding.UTF8.GetBytes(bmsguid.ToString() + " " + ip);
                channel.BasicPublish(exchange: "global",
                                      routingKey: "",
                                      basicProperties: null,
                                      body: body);
                Console.WriteLine("send bmsguid:" + bmsguid);
                return View();
            }
        }
    }
}
