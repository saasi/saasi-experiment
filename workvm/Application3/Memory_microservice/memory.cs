using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MEMORY_Microservice
{
    class memory
    {
        private string id;
        private int time;
        public memory(string id)
        {
            this.id = id;
        }
        static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");
            memory mem1 = new memory("1");
            memory mem2 = new memory("2");

            Thread t1 = new Thread(mem1.MemoryProcessing);
          //  Thread t2 = new Thread(mem2.MemoryProcessing);
            t1.Start();
         //   t2.Start();

        }
        private  void Fun(int time)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Console.WriteLine(this.id + ":Start." + Convert.ToString(currentTime));
            List<IntPtr> alist = new List<IntPtr>();
            int i = 0;
            IntPtr hglobal;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                //list.Add(new byte[1024]); // Change the size here.
                //Thread.Sleep(5); // Change the wait time here.
                //double[,] a = new double[10000, 10000];
                hglobal = Marshal.AllocHGlobal(2000);
                alist.Add(hglobal);
                Thread.Sleep(5); // Change the wait time here.
                
                //i++;
                //if (i == 1000)
                //    list.Clear();
                //   Marshal.FreeHGlobal(hglobal);
            }
            Console.WriteLine("free memory");
            foreach (var item in alist)
            {
                Marshal.FreeHGlobal(item);
            }
            alist = null;
            //Console.WriteLine(i.ToString());
            //alist.Clear();
            //alist = null;
            
            Console.WriteLine(this.id + ":Done." + Convert.ToString(System.DateTime.Now));
        }

        private  void MemoryProcessing()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "direct");
                var queueName = "memory_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);
                channel.QueueBind(queue: queueName, exchange: "call", routingKey: "api");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var order = message.Split(' ');
                    if (order[2].Equals("1"))
                    {
                        int time = Convert.ToInt16(order[3]);
                        //this.Fun(time);
                        worker w = new worker(Guid.NewGuid().ToString(), time, channel, ea);
                        new Thread(w.Fun).Start();
                    }

                };
                channel.BasicConsume(queue: queueName,
                                     noAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Looping...");
                //Console.ReadLine();
                while(true){ Thread.Sleep(5000);};
            }
        }

        public class worker
        {
            private string id;
            private IModel channel;
            private BasicDeliverEventArgs ea;
            private int time;

            public worker(string id, int time, IModel channel, BasicDeliverEventArgs ea)
            {
                this.id = id;
                this.time = time;
                this.channel = channel;
                this.ea = ea;
            }

            public void Fun()
            {
                DateTime currentTime = new DateTime();
                currentTime = System.DateTime.Now;
                DateTime finishTime = currentTime.AddSeconds(time);
                Console.WriteLine(this.id + ":Start." + Convert.ToString(currentTime));
                List<IntPtr> alist = new List<IntPtr>();
                //int i = 0;
                IntPtr hglobal;
                while (System.DateTime.Now.CompareTo(finishTime) < 0)
                {
                    //list.Add(new byte[1024]); // Change the size here.
                    //Thread.Sleep(5); // Change the wait time here.
                    //double[,] a = new double[10000, 10000];
                    hglobal = Marshal.AllocHGlobal(1000);
                    alist.Add(hglobal);
                    Thread.Sleep(2); // Change the wait time here.

                    //i++;
                    //if (i == 1000)
                    //    list.Clear();
                    //   Marshal.FreeHGlobal(hglobal);
                }
                //Console.WriteLine("free memory");
                foreach (var item in alist)
                {
                    Marshal.FreeHGlobal(item);
                }
                alist = null;
                //Console.WriteLine(i.ToString());
                //alist.Clear();
                //alist = null;

                Console.WriteLine(this.id + ":Done." + Convert.ToString(System.DateTime.Now));
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }
    }
}