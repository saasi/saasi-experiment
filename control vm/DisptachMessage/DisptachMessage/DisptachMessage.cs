using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace DisptachMessage
{
    class DisptachMessage
    {
        static ConcurrentBag<string> bmsBag;
        static ConcurrentDictionary<string, string> bmsDic;
        static int count = 0;
        static MSQueue<string> bmsQueue;
        static void Main(string[] args)
        {
            //In the final version, we need to use dictionary to store <bmsguid, ipaddress> so we can send scaleout order to specific DM in vm)
            bmsBag = new ConcurrentBag<string>();
            bmsDic = new ConcurrentDictionary<string, string>();
            bmsQueue = new MSQueue<string>();
            new Thread(urlListener).Start();
            new Thread(bmsListener).Start();
 	    new Thread(globalDMListener).Start();
        }

        static void urlListener()
        {
            Console.WriteLine("urlListener start");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "url", type: "direct");
                var queueName = "url_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "url", routingKey: "dispatch");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    dispatchAsync(message);
                };
                channel.BasicConsume(queue: queueName,
                     noAck: true,
                     consumer: consumer);
                Console.ReadLine();
            }
            
        }

        static void bmsListener() 
        {
            Console.WriteLine("bmsListener start");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "container", type: "direct");
                var queueName = "container_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "container", routingKey: "guid");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var address = message.Split(' ')[0];
                    var bmsguid = message.Split(' ')[1];
                    bmsBag.Add(message);
                    Console.WriteLine(message);

                };
                channel.BasicConsume(queue: queueName,
                     noAck: true,
                     consumer: consumer);
                Console.ReadLine();
            }
        }

        static async System.Threading.Tasks.Task dispatchAsync(string message) // dispatch to bms
        {
            string bmsinfo = "";
            if (!bmsQueue.deque(ref bmsinfo))
            {
                var bmsArray = bmsBag.ToArray();
                foreach (string info in bmsArray)
                {
                    bmsQueue.enqueue(info);
                }
                bmsQueue.deque(ref bmsinfo);
            }


 
            var address = bmsinfo.Split(' ')[0];
            //var bmsguid = bmsinfo.Split(' ')[1];
            var url = "http://" + address + ":5001/" + message;
            var httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            var response =await httpClient.GetAsync(url);
            Console.WriteLine(url + "dispatched");
        }

        static void globalDMListener()
        {
            Console.WriteLine("globaldmListener start");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "global", type: "direct");
                var queueName = "global_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "global", routingKey: "");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var bmsguid = message.Split(' ')[0];
                    var address = message.Split(' ')[1];
                    bmsDic.GetOrAdd(bmsguid, address);
                    Console.WriteLine("bmsIP:" + bmsDic[bmsguid]);
                    Console.WriteLine("message:" + message);
                    sendToDM(bmsguid);
                
                };
                channel.BasicConsume(queue: queueName,
                     noAck: true,
                     consumer: consumer);
                Console.ReadLine();
            }
        }

        static void sendToDM(string bmsguid) // send to DM in vm
        {
           
                var httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = 256000;
                
                var url = "http://" + bmsDic[bmsguid] + ":5002/ScaleOut?bmsguid=" + bmsguid;
                var response = httpClient.GetAsync(url);
                Console.WriteLine("sended:" + url);
           

        }
    }
}
