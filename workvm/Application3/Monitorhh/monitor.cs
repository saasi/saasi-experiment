using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.IO;
using Docker.DotNet;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Reflection;

namespace Monitor
{
    class monitor
    {
        private static double IO_LIMIT;
        private static double CPU_LIMIT;
        private static double MEMORY_LIMIT;
        private static Dictionary<string, int> bms;
        private static string vmaddress = "localhost";
        public static void Main(string[] args)
        {
            // wait for RabbitMQ to be ready
            Console.WriteLine("================== Waiting 5 sec for RabbitMQ");
            Thread.Sleep(5000);
            Console.WriteLine("================== Sleeping done");
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "C:/Program Files/Git/bin/bash.exe", Arguments = "./nameidcollect.sh", };
            Process nameid = new Process() { StartInfo = startInfo, };
            nameid.Start();

            monitor dm = new monitor();
            // client = new DockerClientConfiguration(new Uri("http://192.168.99.100:4550")).CreateClient();
          //  vmaddress = getVmAddress();
            new Thread(monitorBusinessTimeout).Start();
            new Thread(monitorBusinessInfo).Start();
            Console.ReadLine();
            while (true)
            {
                Thread.Sleep(5000);
            }
            /*  while (true)
              {
                  Dictionary<string, string> containers = dm.getContainerList(); //"Adress" : "type"
                  DateTime startTime = new DateTime();
                  startTime = System.DateTime.Now;
                  DateTime finishTime = startTime.AddSeconds(30000);
                  while (System.DateTime.Now.CompareTo(finishTime) < 0)
                  {
                      foreach (KeyValuePair<string, string> container in containers)
                      {
                          if (container.Value.Equals("IO"))
                          {
                              writeRecord("io", container.Key, 0);
                              if (dm.getUsage(container) > IO_LIMIT)
                              {
                                  reportGM("IO");
                                  writeRecord("io");
                              }
                          }


                          if (container.Value.Equals("CPU"))
                          {
                              writeRecord("cpu", container.Key, 0);
                              if (dm.getUsage(container) > CPU_LIMIT)
                              {
                                  reportGM("CPU");
                                  writeRecord("cpu");
                              }
                          }


                          if (container.Value.Equals("MEMORY"))
                          {
                              writeRecord("memory", container.Key, 0);
                              if (dm.getUsage(container) > CPU_LIMIT)
                              {
                                  reportGM("MEMORY");
                                  writeRecord("memory");
                              }
                          }


                      }
                  }


              }*/


        }

        public monitor()
        {
            bms = new Dictionary<string, int>();
        }

        public static string getVmAddress()
        {  //run script
            Process process = new Process();
            process.StartInfo.FileName = "/bin/sh";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.WriteLine("docker ps");
            process.StandardInput.AutoFlush = true;
            process.StandardInput.WriteLine("exit");
            //获取结果

            string strRst = process.StandardOutput.ReadToEnd();
            Console.WriteLine(strRst);
            return strRst;
        }

        public static void getContainerList()
        {
            

                {
                    string assemblyName = Assembly.GetExecutingAssembly().Location;
                    string assemblyDirectory = Path.GetDirectoryName(assemblyName);
                    m_readFile = new StreamReader(assemblyDirectory + @"\" + "name.txt");

                    int counter = 1;
                    string line;
                    while ((line = m_readFile.ReadLine()) != null)
                    {
                        string col = line.Split(' ')[0];
                        MessageBox.Show(line);
                        counter++;
                    }

                    m_readFile.Close();

                }
            }
            public static void monitorBusinessTimeout() //A thread to listen message from DM about bms timeout
        {
            Console.WriteLine("start listening business timeout");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "dm", type: "direct");
                var queueName = channel.QueueDeclare().QueueName;
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
                    if (bms.ContainsKey(message))
                    {
                        bms[message]++;
                        if (bms[message] >=3)
                        {
                            scaleOut("bms");
                            writeRecord(message);
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
        public double getUsage(KeyValuePair<string, string> container)
        {
            //get usage of the container
            return 0;
        }
        public static void scaleOut(string type)
        {

        }
        public static void monitorBusinessInfo()
        {
            Console.WriteLine("start listening business info");
            //var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "call", type: "direct");
                var queueName = "info_queue";
                channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.QueueBind(queue: queueName, exchange: "call", routingKey: "businessinfo");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                    sendBmsInfo(message);

                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Looping ...");
                Console.ReadLine();
                // while (true) { Thread.Sleep(5000); };
            }
        }




        public static void writeRecord(string type, string containerId, double usage) //record cpu/io/memory usage
        {
            StreamWriter sw = File.AppendText(type + ".txt");
            sw.WriteLine(containerId + " " + Convert.ToString(usage) + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void writeRecord(Guid bmsguid) //record bms scaleout
        {
            StreamWriter sw = File.AppendText("business.txt");
            sw.WriteLine(bmsguid.ToString() + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void writeRecord(string type) //record api scaleout
        {
            StreamWriter sw = File.AppendText("api.txt");
            sw.WriteLine(type + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static async System.Threading.Tasks.Task sendBmsInfo(string message)
        {
            try
            {
                
                var url = "http://localhost:5000/BusinessContainer?adress=" + vmaddress + "&bmsguid=" + message;
                Console.WriteLine("send bms info" + url);
                var httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = 256000;
                var response = await httpClient.GetAsync(url);

            }
            catch
            {
                Console.WriteLine("Network Error");
            }
        }
    }
}