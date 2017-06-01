using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace MEMORY_Microservice
{
    class memory
    {
        static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");
            new Thread(memory.MemoryProcessing).Start();
            new Thread(memory.MemoryProcessing).Start();

        }
        private static void Fun(int time)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Console.WriteLine("Start." + Convert.ToString(currentTime));
            List<Object> alist = new List<Object>();
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                double[,] a = new double[8000, 8000];
                alist.Add(a);
            }
            alist.Clear();
            Console.WriteLine("Done." + Convert.ToString(System.DateTime.Now));
        }

        private static void MemoryProcessing()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "fanout");
                var queueName = "memory_queue";
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
                    var order = message.Split(' ');
                    if (order[2].Equals("1"))
                    {
                        int time = Convert.ToInt16(order[3]);
                        memory.Fun(time);
                    }

                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Looping...");
                //Console.ReadLine();
                while(true){ Thread.Sleep(5000);};
            }
        }
    }
}