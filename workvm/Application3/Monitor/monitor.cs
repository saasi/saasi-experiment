using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Monitor
{
    class monitor
    {
        private static int CPUViolationCounter = 0;
        private static int MemoryViolationCounter = 0;
        private static int IOViolationCounter = 0;
        private static double cpuViolationThresdhold = 80.0;
        private static double memoryViolationThreshold = 40.0;
        private static double IOViolationThresdhold = 30.0;
        private static Dictionary<string, int> bms;
        private static Dictionary<string, string> containers;
        private static Dictionary<string, DateTime> scaleTime;
        private static int bmsNum = 1;
        private static int ioNum = 1;
        private static int cpuNum = 1;
        private static int memNum = 1;
        private static string vmaddress; 
        private static Dictionary<string, int> containerViolation;
        private static Dictionary<string, double> io_use_old;
        public static void Main(string[] args)
        {

             vmaddress = getVmAddress();
            scaleTime = new Dictionary<string, DateTime>();
            io_use_old = new Dictionary<string, double>();
             Console.WriteLine("ip"+ vmaddress);
             sendVMInfo();
            monitor dm = new monitor();
            new Thread(monitorBusinessTimeout).Start();
            // new Thread(monitorBusinessInfo).Start();
            containerViolation = new Dictionary<string, int>();
	    // Reset scale = 1 for io/cpu/memory
	    scaleOut("io");
	    scaleOut("cpu");
	    scaleOut("memory");
            Thread.Sleep(5000);
            while (true)
            {
                containers = getContainerList(); //"id" : "image"
                Console.WriteLine("Update container list");
                DateTime startTime = new DateTime();
                startTime = System.DateTime.Now;
                DateTime finishTime = startTime.AddSeconds(20);               
                while (System.DateTime.Now.CompareTo(finishTime) < 0)
                {
                   foreach (KeyValuePair<string, string> container in containers)
                   {
                        if (!containerViolation.ContainsKey(container.Key))
                            containerViolation.Add(container.Key, 0);
                       if (container.Value.Equals("io_microservice"))
                       {
                            var usage = getUsage(container,"io");
                            
                           if (usage > IOViolationThresdhold)
                           {
                                
                                containerViolation[container.Key]++;
                                Console.WriteLine("io volation:" + containerViolation[container.Key]);
                                
                                if (containerViolation[container.Key] >=3)
                                {
                                    if (!scaleTime.ContainsKey(container.Key) || scaleTime[container.Key].AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
                                    {
                                        ioNum++;
                                        writeRecord("io");
                                        scaleOut("io");
                                        if (!scaleTime.ContainsKey(container.Key))
                                            scaleTime.Add(container.Key, DateTime.Now);
                                        else
                                            scaleTime[container.Key] = DateTime.Now;
                                        
                                    }
                                    containerViolation[container.Key] = 0;
                                }
                               
                           }
                       }


                       if (container.Value.Equals("cpu_microservice"))
                       {
                            var usage = getUsage(container,"cpu");
                            if (usage > cpuViolationThresdhold)
                            {
                                
                                containerViolation[container.Key]++;
                                Console.WriteLine("cpu volation:" + containerViolation[container.Key]);
                                if (containerViolation[container.Key] >= 3)
                                {
                                    if (!scaleTime.ContainsKey(container.Key) || scaleTime[container.Key].AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
                                    {
                                        cpuNum++;
                                        writeRecord("cpu");
                                        scaleOut("cpu");
                                        if (!scaleTime.ContainsKey(container.Key))
                                            scaleTime.Add(container.Key, DateTime.Now);
                                        else
                                            scaleTime[container.Key] = DateTime.Now;
                                        
                                    }
                                    containerViolation[container.Key] = 0;

                                }
                            }
                       }


                       if (container.Value.Equals("memory_microservice"))
                       {
                           var usage = getUsage(container,"memory");
                           if (usage > memoryViolationThreshold)
                           {
                                
                                containerViolation[container.Key]++;
                                Console.WriteLine("memory volation:" + containerViolation[container.Key]);
                                if (containerViolation[container.Key] >= 3)
                                {
                                    if (!scaleTime.ContainsKey(container.Key) || scaleTime[container.Key].AddSeconds(30).CompareTo(DateTime.Now) < 0) //A container can scale one time in one minute.
                                    {
                                        memNum++;
                                        writeRecord("memory");
                                        scaleOut("memory");
                                        if (!scaleTime.ContainsKey(container.Key))
                                            scaleTime.Add(container.Key, DateTime.Now);
                                        else
                                            scaleTime[container.Key] = DateTime.Now;
                                        
                                    }
                                    containerViolation[container.Key] = 0;
                                }
                           }
                       }

                        
                   }
                    Thread.Sleep(2000);
                }


            }


        }

        public monitor()
        {
            bms = new Dictionary<string, int>();
        }
        public static double getUsage(KeyValuePair<string, string> container, string type)
        {
            double usage = 0;
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./stats.sh " + container.Key, };
            Process ip = new Process() { StartInfo = startInfo };
            try
            {
                ip.Start();
                Thread.Sleep(500);
                var lines = File.ReadAllLines(@"stats.txt");
  
                List<string> list = new List<string>();
                foreach (string s in lines)
                {
                    if (s.Contains(container.Key))
                    {
                        list.Add(s);
                    }
                }
                var line = list.ToArray()[0].Split(' ');

                List<string> list2 = new List<string>();
                foreach (string s in line)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        list2.Add(s);
                    }
                }
                var data = list2.ToArray();
               // for (int i = 0; i < data.Length; i++)
               // {
              //      Console.WriteLine(data[i]);
              //  }
                if (type.Equals("cpu"))
                {
                    usage = Convert.ToDouble(data[1].Substring(0, data[1].Length - 1));
                }

                if (type.Equals("memory"))
                {
                    usage = Convert.ToDouble(data[7].Substring(0, data[7].Length - 1));
                }

                if (type.Equals("io"))
                {
                    if (!io_use_old.ContainsKey(container.Key))
                        io_use_old.Add(container.Key, 0);
                    double use = 0;
                    if (data[17].Equals("MB"))
                    {
                        use = Convert.ToDouble(data[16]);
                    }
                    else if (data[17].Equals("GB"))
                        use = Convert.ToDouble(data[16]) * 1000;

                    Console.WriteLine(use.ToString());
                    if (use == 0)
                        use = io_use_old[container.Key];
                    usage = (use - io_use_old[container.Key]) / 3;
                    Console.WriteLine(container.Key + ":" + io_use_old[container.Key] + " " + use);
                    io_use_old[container.Key] = use;
                    
                }
            }catch
            {
                Console.WriteLine("read error");
            }
            finally
            {
                // Must kill the docker stats process. Otherwise CPU will be used up.
                try {
                    ip.Kill();
                    ip.Dispose();
                } catch {
                    //do nothing
                }
                
            }

            Console.WriteLine(type + ":" +container.Key+":"+ usage.ToString());
            writeRecord(type, container.Key, usage);
            return usage;

        }

        public static String getVmAddress()
        {
            String vmip;
            ProcessStartInfo startInfo2 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./getvmip2.sh", };
            Process ip = new Process() { StartInfo = startInfo2, };
            ip.Start();
            Thread.Sleep(1000);
            vmip = System.IO.File.ReadAllText(@"vmip.txt");
            //Console.WriteLine("vmip:" + vmip);
            return vmip;
        }

        public static Dictionary<string, string> getContainerList()
        {
            Dictionary<string, string> tempContainers = new Dictionary<string, string>();
            ProcessStartInfo startInfo2 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./nameidcollect.sh", };
            Process imgname = new Process() { StartInfo = startInfo2, };
            imgname.Start();

            Thread.Sleep(1000);
            string[] Info = System.IO.File.ReadAllLines(@"name.txt");
            foreach (string container in Info)
            {
                tempContainers.Add(container.Split(' ')[0], container.Split(' ')[1]);
                Console.WriteLine(container.Split(' ')[0] + " " + container.Split(' ')[1]);
            }

            return tempContainers;

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
                        if (bms[message] >5)
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
                { FileName = "/bin/bash", Arguments = "./scalebms1.sh " + bmsNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

            if (type.Equals("io"))
            {
                Console.WriteLine("scaleout io");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scaleio1.sh " + ioNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

            if (type.Equals("cpu"))
            {
                Console.WriteLine("scaleout cpu");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scalecpu1.sh " + cpuNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

            if (type.Equals("memory"))
            {
                Console.WriteLine("scaleout memory");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scalemem1.sh " + memNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

        }

     /*   public static void monitorBusinessInfo()
        {
            Console.WriteLine("start listening business info");
            //var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
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
        }*/




        public static void writeRecord(string type, string containerId, double usage) //record cpu/io/memory usage
        {
            StreamWriter sw = File.AppendText("data/apiStats.txt");
            sw.WriteLine(type + " " + containerId + " " + Convert.ToString(usage) + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void writeRecord(Guid bmsguid) //record bms scaleout
        {
            StreamWriter sw = File.AppendText("data/business-scaleout.txt");
            sw.WriteLine(bmsguid.ToString() + " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void writeRecord(string type) //record api scaleout
        {
            StreamWriter sw = File.AppendText("data/api-scaleout.txt");
            sw.WriteLine(type + " " + Convert.ToString(System.DateTime.Now));
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

        public static void sendVMInfo()
        {
            try
           {

            var url = "http://10.137.0.81:5000/BusinessContainer?adress=" + vmaddress;
            Console.WriteLine("send bms info" + url);
            var httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            var response = httpClient.GetAsync(url);

            }
            catch
            {
                Console.WriteLine("Network Error");
            }
        }
    }
}
