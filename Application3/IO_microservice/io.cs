using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace IO_Microservice
{
    class io
    {
        static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");

            new Thread(io.IoProcessing).Start();
            new Thread(io.IoProcessing).Start();
            // new Thread(io.IoProcessing).Start();

        }

        private static void Fun(int time)
        {

            String st = Guid.NewGuid().ToString();
            FileStream fs = new FileStream("write" + Convert.ToString(st) + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Console.WriteLine("Start." + Convert.ToString(currentTime));
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                String s = io.GenerateRandomString(800);
                sw.Write(s);
            }

            fs.Flush();
            fs.Dispose();
            Console.WriteLine("Done." + Convert.ToString(System.DateTime.Now));
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
        public static void IoProcessing()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "fanout");
                var queueName = "io_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "call", routingKey: "");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                    var order = message.Split(' ');
                    if (order[0].Equals("1"))
                    {
                        int time = Convert.ToInt16(order[3]);
                        io.Fun(time);
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