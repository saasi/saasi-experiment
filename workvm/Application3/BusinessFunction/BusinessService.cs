using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BusinessFunction
{
    class BusinessService
    {
        private static Guid bmsGuid;
        private static readonly string _rabbitMQHost = "rabbitmq";
        static void Main(string[] args)
        {
            // Wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting for RabbitMQ to start");


            var factory = new ConnectionFactory() { HostName = _rabbitMQHost };
            var connected = false;
            while (!connected)
            {
                try
                {
                    using (var connection = factory.CreateConnection())
                    {
                        Console.WriteLine("================== Connected");
                        connected = true;
                    }

                }
                catch (BrokerUnreachableException e)
                {
                    // not connected
                    Console.WriteLine("================== Not connected, retrying in 500ms");
                }
                Thread.Sleep(500);
            }
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
                channel_mono.BasicQos(prefetchSize: 0, prefetchCount: 15, global: false);
                channel_mono.QueueBind(queue: queueName, exchange: "mono", routingKey: "business");
                var consumer = new EventingBasicConsumer(channel_mono);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                    Console.WriteLine(bmsGuid + ":call api");
                    CallApi(message);
                    //channel_mono.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
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
                Guid id = Guid.NewGuid();
               // 
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                DateTime startTime = DateTime.Now;
                try
                {
                    startTime = dtDateTime.AddSeconds(Convert.ToDouble(message.Split(' ')[5]));
                }
                catch
                {

                }
                
                //Console.WriteLine(id + " business start:" + startTime.ToString());
                var timetorun = Convert.ToInt16(message.Split(' ')[3]);
                var timeout = Convert.ToInt16(message.Split(' ')[4]);
                var recieveTime = DateTime.Now;
                Thread.Sleep(timetorun * 1000);
                //DateTime beginTime = System.DateTime.Now;
                //DateTime finishTime = beginTime.AddSeconds(timetorun);
                
                //DateTime finishTime = startTime.AddSeconds(timetorun);

                DateTime completeRunTime = System.DateTime.Now;
                Console.WriteLine(id + " " + recieveTime.ToString() + " " + startTime.ToString() + " " + completeRunTime.ToString() + " " + startTime.AddSeconds(timeout).ToString());
                //Console.WriteLine(startTime.AddSeconds(timeout).ToString() + " " + completeRunTime.ToString());
                if (startTime.AddSeconds(timeout).CompareTo(completeRunTime) < 0) //check timeout
                //if (startTime.AddSeconds(timeout).CompareTo(completeRunTime) < 0)
                {
                    var message2 = bmsGuid;
                    Console.WriteLine("send to GlobalDM:" + message2);
                    var httpClient = new HttpClient();
                    httpClient.MaxResponseContentBufferSize = 256000;
                    var response = httpClient.GetAsync("http://10.137.0.81:5001/BusinessTimeout?bmsguid=" + message2);
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

         /*       var factory = new ConnectionFactory() { HostName = _rabbitMQHost };
                using (var connection = factory.CreateConnection())
                using (var channel_mono = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "reply", type: "direct");
                    string reply = "done";
                    var body = Encoding.UTF8.GetBytes(reply);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish(exchange: "mono",
                                          routingKey: messageGuid,
                                          basicProperties: properties,
                                          body: body);
                }*/
                    
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
                var order = message.Split(' ');
                if (order[0].Equals("1"))
                {
                    channel_api.BasicPublish(exchange: "call",
                  routingKey: "io",
                  basicProperties: null,
                  body: body);
                }
                if (order[1].Equals("1"))
                {
                    channel_api.BasicPublish(exchange: "call",
                  routingKey: "cpu",
                  basicProperties: null,
                  body: body);
                }
                if (order[2].Equals("1"))
                {
                    channel_api.BasicPublish(exchange: "call",
                  routingKey: "memory",
                  basicProperties: null,
                  body: body);
                }
                Console.WriteLine(message);
                Console.WriteLine("Send to API microservice");
            }
        }
    }
}
