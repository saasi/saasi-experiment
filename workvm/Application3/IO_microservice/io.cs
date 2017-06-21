using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace IO_Microservice
{
    class io
    {
        private int id;
        public io(int id)
        {
            this.id = id;
        }
        static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");
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
            String fileName = "write" + Convert.ToString(st) + ".txt";
            FileStream fs = new FileStream(fileName, FileMode.Create);
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {

                StreamWriter sw = new StreamWriter(fs);
               // String s = io.GenerateRandomString(2000);
               // sw.Write("1111111111111111111111111111111111111111111111");
               // fs.Flush();
                var httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = 256000;
                
                var url = "http://localhost:5001";
                var response = httpClient.GetAsync(url);
                Thread.Sleep(100); 
                //System.IO.Directory.Delete(fileName);
            }
            //fs.Dispose();



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
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
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