using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BusinessFunction
{
    class BusinessService
    {
        private static Guid bmsGuid;
        static void Main(string[] args)
        {
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");
            bmsGuid = Guid.NewGuid();
            new Thread(businessProcessing).Start();
            //
        }
        static void sendBMSInfo()  //send bms guid to monitor
        {
            
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel_api = connection.CreateModel())
            {
                channel_api.ExchangeDeclare(exchange: "call", type: "direct");
                var message = bmsGuid.ToString();
                var body = Encoding.UTF8.GetBytes(message);
                //var properties = channel_api.CreateBasicProperties();
                //properties.Persistent = true;
                channel_api.BasicPublish(exchange: "call",
                                      routingKey: "businessinfo",
                                      basicProperties: null,
                                      body: body);
                Console.WriteLine(message + ":Send to Monitor");
            }
        }
        static void businessProcessing()
        {
            
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel_mono = connection.CreateModel())
            {
                DateTime startRunTime = System.DateTime.Now;
                Console.WriteLine("waiting call");
                channel_mono.ExchangeDeclare(exchange: "mono", type: "direct");
                var queueName = "business_queue";
                channel_mono.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel_mono.BasicQos(prefetchSize: 0, prefetchCount: 8, global: false);
                channel_mono.QueueBind(queue: queueName, exchange: "mono", routingKey: "business");
                var consumer = new EventingBasicConsumer(channel_mono);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                    Console.WriteLine(bmsGuid + ":call api");
                    CallApi(message);
                    businessTask bt = new businessTask(message, channel_mono, ea);
                    //Thread t = new Thread(bt.Fun);
                    //t.Start();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(bt.Fun));


                };
                channel_mono.BasicConsume(queue: queueName,
                                     noAck: false,
                                     consumer: consumer);

              /*   if (startRunTime.AddSeconds(timeout).CompareTo(completeRunTime) > 0) //check timeout
                    Console.WriteLine("send to DM");
                    var message2 = Convert.ToString(id) + " " + guid;
                    var body2 = Encoding.UTF8.GetBytes(message2);
                    channel.BasicPublish(exchange: "call",
                          routingKey: "report",
                          basicProperties: null,
                          body: body2);*/
                          
                while (true) { Thread.Sleep(1000); };
            }
            
        }
        class businessTask
        {
            private string message;
            private IModel channel;
            private BasicDeliverEventArgs ea;
            public businessTask(string message, IModel channel, BasicDeliverEventArgs ea)
            {
                this.message = message;
                this.channel = channel;
                this.ea = ea;
            }
            public void Fun(object state)
            {
                DateTime currentTime = System.DateTime.Now;
                Console.WriteLine("business start:" + currentTime.ToString());
                var timetorun = Convert.ToDouble(message.Split(' ')[3]);
                var timeout = Convert.ToDouble(message.Split(' ')[4]);
                DateTime finishTime = currentTime.AddSeconds(timetorun);
                while (System.DateTime.Now.CompareTo(finishTime) < 0)
                {
                    GenerateRandomString(10);
                }
                DateTime completeRunTime = System.DateTime.Now;
                Console.WriteLine("business end:" + completeRunTime.ToString());
                Console.WriteLine(currentTime.AddSeconds(timeout).ToString() + " " + completeRunTime.ToString());
                if (currentTime.AddSeconds(timeout).CompareTo(completeRunTime) < 0) //check timeout
                {
                    var message2 = bmsGuid;
                    Console.WriteLine("send to GlobalDM:" + message2);
                    var httpClient = new HttpClient();
                    httpClient.MaxResponseContentBufferSize = 256000;
                    var response = httpClient.GetAsync("http://10.137.0.81:5001/BusinessTimeout?bmsguid=" + message2);
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
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
        public static void CallApi(String message)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel_api = connection.CreateModel())
            {
                channel_api.ExchangeDeclare(exchange: "call", type: "direct");
                var body = Encoding.UTF8.GetBytes(message);
                //var properties = channel_api.CreateBasicProperties();
                //properties.Persistent = true;
                channel_api.BasicPublish(exchange: "call",
                                      routingKey: "api",
                                      basicProperties: null,
                                      body: body);
                Console.WriteLine("Send to API microservice");
            }
        }
    }
}
