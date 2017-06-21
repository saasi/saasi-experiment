using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;
using System.Text;
using System.Threading;

namespace CPU_Microservice
{
    class CPU
    {
        static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");

            new Thread(CPU.CpuProcessing).Start();
            new Thread(CPU.CpuProcessing).Start();
        }

        public static void Fun(int time)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Console.WriteLine("Start." + Convert.ToString(currentTime));
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                string comparestring1 = StringDistance.GenerateRandomString(200);
                string comparestring2 = StringDistance.GenerateRandomString(200);
                StringDistance.LevenshteinDistance(comparestring1, comparestring2);
            }
            Console.WriteLine("Done." + Convert.ToString(System.DateTime.Now));

        }
        public static void CpuProcessing()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "fanout");
                var queueName = "cpu_queue";
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
                    if (order[1].Equals("1"))
                    {
                        int time = Convert.ToInt16(order[3]);
                        CPU.Fun(time);
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
    internal class StringDistance
    {
        #region Public Methods

        public static string GenerateRandomString(int length)
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
        public static int LevenshteinDistance(string str1, string str2)
        {
            var scratchDistanceMatrix = new int[str1.Length + 1, str2.Length + 1];
            // distance matrix contains one extra row and column for the seed values         
            for (int i = 0; i <= str1.Length; i++)
            {
                scratchDistanceMatrix[i, 0] = i;
            }
            for (int j = 0; j <= str2.Length; j++)
            {
                scratchDistanceMatrix[0, j] = j;
            }
            for (int i = 1; i <= str1.Length; i++)
            {
                int str1Index = i - 1;
                for (int j = 1; j <= str2.Length; j++)
                {
                    int str2Index = j - 1;
                    int cost = (str1[str1Index] == str2[str2Index]) ? 0 : 1;
                    int deletion = (i == 0) ? 1 : scratchDistanceMatrix[i - 1, j] + 1;
                    int insertion = (j == 0) ? 1 : scratchDistanceMatrix[i, j - 1] + 1;
                    int substitution = (i == 0 || j == 0) ? cost : scratchDistanceMatrix[i - 1, j - 1] + cost;
                    scratchDistanceMatrix[i, j] = Math.Min(Math.Min(deletion, insertion), substitution);
                    // Check for Transposition  
                    if (i > 1 && j > 1 && (str1[str1Index] == str2[str2Index - 1]) &&
                        (str1[str1Index - 1] == str2[str2Index]))
                    {
                        scratchDistanceMatrix[i, j] = Math.Min(
                            scratchDistanceMatrix[i, j], scratchDistanceMatrix[i - 2, j - 2] + cost);
                    }
                }
            }
            // Levenshtein distance is the bottom right element       
            return scratchDistanceMatrix[str1.Length, str2.Length];
        }
        #endregion
    }
}