using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using RabbitMQ.Client.Exceptions;

namespace IO_Microservice
{
    class io
    {
        private static readonly string _rabbitMQHost = "rabbitmq";
        private static readonly long _fileSize = 10L * 1024L * 1024L * 1024L; //10 G
        private int id;
        public io(int id)
        {
            this.id = id;
        }
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

            io io1 = new io(1);
            Thread t1 = new Thread(io1.IoProcessing); //listen message
            t1.Start();

        }

        public  void IoProcessing()
        {
            var factory = new ConnectionFactory() { HostName = _rabbitMQHost };
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "direct");
                var queueName = "io_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 30, global: false);
                channel.QueueBind(queue: queueName, exchange: "call", routingKey: "io");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var order = message.Split(' ');

                    int time = Convert.ToInt16(order[3]);
                    Worker w = new Worker(Guid.NewGuid().ToString(), time, channel, ea);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(w.Fun));

                };
                channel.BasicConsume(queue: queueName,
                                     noAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Looping ...");
                //Console.ReadLine();
                while(true){ Thread.Sleep(10);};
            }
        }

        public class Worker
        {
            private int time;
            private string id;
            private IModel channel;
            private BasicDeliverEventArgs ea;
            public Worker(string id, int time, IModel channel, BasicDeliverEventArgs ea)
            {
                this.time = time;
                this.channel = channel;
                this.ea = ea;
                this.id = id;
            }
            public void Fun(object state)
            {
                DateTime currentTime = new DateTime();
                currentTime = System.DateTime.Now;
                DateTime finishTime = currentTime.AddSeconds(time);
                Console.WriteLine(this.id.ToString() + ":Start." + Convert.ToString(currentTime));
                String st = Guid.NewGuid().ToString();
                String fileName = "write" + st + ".tmp";
                FileStream fs = new FileStream(fileName, FileMode.Create);
                fs.SetLength(_fileSize);
                StreamWriter sw = new StreamWriter(fs);
                while (System.DateTime.Now.CompareTo(finishTime) < 0)
                {


                    String s = io.GenerateRandomString(1000);
                    sw.Write(s);
                    fs.Flush(true);
                    //Thread.Sleep(3); 
                }
                sw.Dispose();

                fs.Dispose();
                var fi = new System.IO.FileInfo(fileName);
                fi.Delete();
                Console.WriteLine(this.id + ":Done." + Convert.ToString(System.DateTime.Now));
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
    }
}