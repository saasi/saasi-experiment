using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using Docker.DotNet;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Monitor
{
    class Monitor
    {
        private static DockerClient dockerClient;
        private static string vmaddress;
        private static IOMicroservice ioMicroservice;
        private static MemoryMicroservice memoryMicroservice;
        private static CPUMicroservice cpuMicroservice;
        private static Dictionary<string, int> bms = new Dictionary<string, int>();
        private static Dictionary<string, DateTime> scaleTime = new Dictionary<string, DateTime>();
        private static int bmsNum = 1;

        public static void Main(string[] args)
        {
            try
            {
                dockerClient = new DockerClientConfiguration(new Uri("http://127.0.0.1:4243"))
                                .CreateClient();
                // dockerClient = new DockerClientConfiguration(new Uri("http://192.168.0.1:4243"))
                //  .CreateClient();
            }
            catch
            {
                Console.WriteLine("Could not connect to Docker Remote API.");
                return;
            }


            new Thread(monitorBusinessTimeout).Start();

            ioMicroservice = new IOMicroservice(dockerClient); // monitor io_microservice
           // cpuMicroservice = new CPUMicroservice(dockerClient);// monitor cpu_microservice
          //  memoryMicroservice = new MemoryMicroservice(dockerClient);// monitor memory_microservice

            while (true)
            {
                Thread.Sleep(5000);
                Console.WriteLine($"##########{DateTime.Now}##########");
              //  Console.WriteLine($"CPU {cpuMicroservice.ActualScale}->{cpuMicroservice.ScaleTarget}");
                Console.WriteLine($"IO {ioMicroservice.ActualScale}->{ioMicroservice.ScaleTarget}");
              //  Console.WriteLine($"Memory {memoryMicroservice.ActualScale}->{memoryMicroservice.ScaleTarget}");
                Console.WriteLine("###########################################");
            }

        }

        public Monitor()
        {
          
        }



        public static void monitorBusinessTimeout() //A thread to listen message from DM about bms timeout
        {

            Console.WriteLine("start listening business timeout");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "dm", type: "direct");
                var queueName = "monitor_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "dm", routingKey: "scaleout");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("scelout:" + message);
                    if (bms.ContainsKey(message))
                    {
                        bms[message]++;
                        writeBmsViolation(Guid.Parse(message));
                        if (bms[message] > 5)
                        {
                            if (!scaleTime.ContainsKey(message) || scaleTime[message].AddSeconds(60).CompareTo(DateTime.Now) < 0)
                            {
                                bmsNum++;
                                Console.WriteLine("scaleouting");
                                scaleOut("bms");
                                if (!scaleTime.ContainsKey(message))
                                    scaleTime.Add(message, DateTime.Now);
                                else
                                    scaleTime[message] = DateTime.Now;
                                writeRecord(Guid.Parse(message));

                            }
                            bms[message] = 0;
                        }
                    }

                    else
                        bms.Add(message, 1);

                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Looping ...");
                Console.ReadLine();
                // while (true) { Thread.Sleep(5000); };
            }
            while (true) { Thread.Sleep(5000); };
        }



        public static void scaleOut(string type)
        {
            //scalebms
            if (type.Equals("bms"))
            {
                Console.WriteLine("scaleout bms");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scalebms1.sh " + bmsNum }; //scale bms
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

        }



        public static void writeRecord(Guid bmsguid) //record bms scaleout
        {
            StreamWriter sw = File.AppendText("data/business-scaleout.txt");
            sw.WriteLine(bmsguid.ToString() + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void writeBmsViolation(Guid bmsguid)
        {
            StreamWriter sw = File.AppendText("data/business-violation.txt");
            sw.WriteLine(bmsguid + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

    }
}
