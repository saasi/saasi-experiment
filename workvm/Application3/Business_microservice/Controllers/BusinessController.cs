<<<<<<< HEAD
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
using RabbitMQ.Client.Events;


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
        //public IActionResult Index(Guid guid, DateTime timestart, int? io = 0, int? cpu = 0, int? memory = 0, int timetorun = 0, int id = 0, int timeout = 0)
        public IActionResult Index(Guid guid, String timestart, int? io = 0, int? cpu = 0, int? memory = 0, int timetorun = 0, int id = 0, int timeout = 0)
        {

            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            Guid messageGuid = Guid.NewGuid();
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "mono", type: "direct");
                string message = Convert.ToString(io) + " " + Convert.ToString(cpu) + " " + Convert.ToString(memory) + " " + Convert.ToString(timetorun) + " " + Convert.ToString(timeout) + " " + timestart + " " + messageGuid;
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
            /*    using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "reply", type: "direct");
                    var queueName = "reply_queue";
                    channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
                    channel.QueueBind(queue: queueName, exchange: "mono", routingKey: messageGuid.ToString());
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                    };

                }*/
            return View();
        }
        
    }
}

=======
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

>>>>>>> 56c41dfd92d212d6ee25ba4b6944e66947b04fde
