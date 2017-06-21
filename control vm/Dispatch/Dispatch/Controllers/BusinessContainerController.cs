using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dispatch.Controllers
{
    public class BusinessContainerController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string adress)
        {
            //ViewData["url"] = info;
            Guid bmsguid = Guid.NewGuid();
            Console.WriteLine(adress + " " + bmsguid);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //send container information to dispatch
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "container", type: "direct");
                var body = Encoding.UTF8.GetBytes(adress + " " + bmsguid);
                channel.BasicPublish(exchange: "container",
                                      routingKey: "guid",
                                      basicProperties:null,
                                      body: body);
                
                return View();
            }

            
        }
    }
}
