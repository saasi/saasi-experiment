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
            Thread t1 = new Thread(io1.IoProcessing);
            //  io io2 = new io(2);
            //  Thread t2 = new Thread(io2.IoProcessing);
            //  new Thread(io2.IoProcessing);
               t1.Start();
            //   t2.Start();
            //io1.Fun(30);

            // new Thread(io.IoProcessing).Start();
            //Console.ReadLine();
        }

        private void Fun(int time)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Console.WriteLine(this.id.ToString() + ":Start." + Convert.ToString(currentTime));
            String st = Guid.NewGuid().ToString();
            String fileName = "write" + Convert.ToString(st) + ".tmp";
            FileStream fs = new FileStream(fileName, FileMode.Create);
            fs.SetLength(_fileSize);
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {

                StreamWriter sw = new StreamWriter(fs);
                String s = io.GenerateRandomString(2000);
                sw.Write(s);
                fs.Flush();
                //Thread.Sleep(100); 
            }
            fs.Dispose();
            System.IO.Directory.Delete(fileName);
            Console.WriteLine(this.id+":Done." + Convert.ToString(System.DateTime.Now));
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
                channel.BasicQos(prefetchSize: 0, prefetchCount: 4, global: false);
                channel.QueueBind(queue: queueName, exchange: "call", routingKey: "api");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var order = message.Split(' ');
                    if (order[0].Equals("1"))
                    {
                        int time = Convert.ToInt16(order[3]);
                        this.Fun(time);
                    }
                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Looping ...");
                //Console.ReadLine();
                while(true){ Thread.Sleep(5000);};
            }
        }
    }
}