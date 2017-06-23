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
        private static double cpuViolationThresdhold = 90.0;
        private static double memoryViolationThreshold = 50.0;
        private static double IOViolationThresdhold = 10.0;
        private static Dictionary<string, int> bms;
        private static Dictionary<string, string> containers;
        private static int bmsNum = 1;
        private static int ioNum = 1;
        private static int cpuNum = 1;
        private static int memNum = 1;
        private static string vmaddress; 
        private static Dictionary<string, int> containerViolation;
        public static void Main(string[] args)
        {

             vmaddress = getVmAddress();
             Console.WriteLine("ip"+ vmaddress);
             sendVMInfo();
            monitor dm = new monitor();
            new Thread(monitorBusinessTimeout).Start();
            // new Thread(monitorBusinessInfo).Start();
            containerViolation = new Dictionary<string, int>();
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
                                writeRecord("io", container.Key, usage);
                                if (containerViolation[container.Key] >=5)
                                {
                                    ioNum++;
                                    writeRecord("io");
                                    scaleOut("io");
                                    containerViolation[container.Key] = 0;
                                }
                               
                           }
                       }


                       if (container.Value.Equals("cpu_microservice"))
                       {
                            var usage = getUsage(container,"cpu");

                           if (usage > cpuViolationThresdhold)
                           {
                                writeRecord("cpu", container.Key, usage);
                                containerViolation[container.Key]++;
                                Console.WriteLine("cpuvolation:" + containerViolation[container.Key]);
                                if (containerViolation[container.Key] >= 5)
                                {
                                    cpuNum++;
                                    writeRecord("cpu");
                                    scaleOut("cpu");
                                    containerViolation[container.Key] = 0;
                                }
                            }
                       }


                       if (container.Value.Equals("memory_microservice"))
                       {
                            var usage = getUsage(container,"memory");

                           if (usage > memoryViolationThreshold)
                           {
                                Console.WriteLine("writememory:" + usage);
                                writeRecord("memory", container.Key, usage);
                                containerViolation[container.Key]++;
                                if (containerViolation[container.Key] >= 5)
                                {
                                    memNum++;
                                    writeRecord("memory");
                                    scaleOut("memory");
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

            
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./stats.sh " + container.Key, };
                Console.WriteLine("./stats.sh " + container.Key);
                Process ip = new Process() { StartInfo = startInfo, };
                ip.Start();
                Thread.Sleep(500);
                var lines = File.ReadAllLines(@"stats.txt");
  
               // System.IO.Directory.Delete("stats.txt");
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
                    var total = Convert.ToDouble(data[13]);
                    if (data[16].Equals("MB"))
                    {
                        var use = Convert.ToDouble(data[15]) / 1000;
                        usage = use / total;
                    }
                    if (data[16].Equals("KB"))
                    {
                        var use = Convert.ToDouble(data[15]) / 1000000;
                        usage = use / total;
                    }

                }
            }catch
            {
                Console.WriteLine("read error");
            }

            Console.WriteLine(type + ":" +container.Key+":"+ usage.ToString());
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
                        if (bms[message] >1)
                        {
                            bmsNum++;
                            Console.WriteLine("scaleouting");
                            scaleOut("bms");
                            writeRecord(Guid.Parse(message));
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


        public static void monitorUsage()
        {
            ProcessStartInfo statInfo1 = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = "./statscollect.sh", };
            Process stat = new Process() { StartInfo = statInfo1, };
            stat.Start();
            string[] lines = File.ReadAllLines(@"stats.txt");

            for (int intCounter = 0; intCounter < lines.Length; intCounter++)
            {
                //each line
                string currentLine = lines[intCounter];
                Console.WriteLine("current line:" + currentLine);
                string[] fields = currentLine.Split('\t');
                // int fieldCounter = 0;
                String currentValue;
                
                for (int fieldCounter = 0; fieldCounter < fields.Length; fieldCounter++)
                {
                    //fieldCounter = 0
                    //get CPU
                    Console.WriteLine(fieldCounter.ToString());
                    if (fieldCounter == 1)
                    {
                        //cpu
                        currentValue = fields[fieldCounter];
                        Console.WriteLine("current value:" + currentValue);
                        String CPUValue = currentValue.Replace('%', ' ');
                        Console.WriteLine("cpu:" + CPUValue);
                        Double dblCPU = Double.Parse(CPUValue);
                        if (dblCPU > cpuViolationThresdhold)
                        {
                            CPUViolationCounter++;
                            Console.WriteLine("CPU Violation");
                            if (CPUViolationCounter >= 3)
                            {
                                //scale out CPU
                                ProcessStartInfo startInfo1 = new ProcessStartInfo()
                                { FileName = "/bin/bash", Arguments = "./scalecpu1.sh", }; //script files should be contained in the application directory
                                Process cpuscale = new Process() { StartInfo = startInfo1, };
                                cpuscale.Start();

                                //Collect Stats of New Containers
                                ProcessStartInfo startInfo2 = new ProcessStartInfo()
                                { FileName = "/bin/bash", Arguments = "./infocollect_onscale.sh", }; //Console Application 1 (Which runs the scripts to collect container info should be in the directory of this app)
                                Process onscalestat = new Process() { StartInfo = startInfo2, };
                                onscalestat.Start();
                                writeRecord("cpu");
                                CPUViolationCounter = 0;
                            }
                        }
                        //memory

                        if (fieldCounter == 3)
                        {
                            currentValue = fields[fieldCounter];
                            String memValue = currentValue.Replace('%', ' ');
                            Console.WriteLine("mem:" + memValue);
                            Double dblmem = Double.Parse(memValue);
                            if (dblmem > memoryViolationThreshold)
                            {
                                Console.WriteLine("memory Violation");
                                MemoryViolationCounter++;
                                if (MemoryViolationCounter >= 3)
                                {
                                    //Scale Memory
                                    ProcessStartInfo startInfo2 = new ProcessStartInfo()
                                    { FileName = "/bin/bash", Arguments = "./scalemem1", };
                                    Process memscale = new Process() { StartInfo = startInfo2, };
                                    memscale.Start();

                                    //Collect Stats of New Containers
                                    ProcessStartInfo startInfo3 = new ProcessStartInfo()
                                    { FileName = "/bin/bash", Arguments = "./infocollect_onscale.sh", }; //Console Application 1 (Which runs the scripts to collect container info should be in the directory of this app)
                                    Process onscalestat = new Process() { StartInfo = startInfo2, };
                                    onscalestat.Start();
                                    writeRecord("memory");
                                    MemoryViolationCounter = 0;
                                }
                            }


                            if (fieldCounter == 4)
                            {
                                currentValue = fields[fieldCounter];
                                String IOValue = currentValue.Replace('B', ' ');
                                Console.WriteLine("io:" + IOValue);
                                Double dblIO = Double.Parse(IOValue);
                                if (dblIO > IOViolationThresdhold)
                                {
                                    Console.WriteLine("IO Violation");
                                    IOViolationCounter++;
                                    if (IOViolationCounter >= 3)
                                    {
                                        //Scale IO
                                        ProcessStartInfo startInfo3 = new ProcessStartInfo()
                                        { FileName = "bin/bash", Arguments = "./scaleio1", };
                                        Process ioscale = new Process() { StartInfo = startInfo3, };
                                        ioscale.Start();

                                        //Collect Stats of New Containers
                                        ProcessStartInfo startInfo2 = new ProcessStartInfo()
                                        { FileName = "/bin/bash", Arguments = "./infocollect_onscale.sh", }; //Console Application 1 (Which runs the scripts to collect container info should be in the directory of this app)
                                        Process onscalestat = new Process() { StartInfo = startInfo2, };
                                        onscalestat.Start();
                                        writeRecord("io");
                                        IOViolationCounter = 0;
                                    }

                                }

                            }






                        }
                    }


                   
                }
            }
        
    
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
                Console.WriteLine("scaleout bms");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scaleio1.sh " + ioNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

            if (type.Equals("cpu"))
            {
                Console.WriteLine("scaleout bms");
                ProcessStartInfo statInfo1 = new ProcessStartInfo()
                { FileName = "/bin/bash", Arguments = "./scalecpu1.sh " + cpuNum }; //Again, scriptfile should be in working directory
                Process stat = new Process() { StartInfo = statInfo1, };
                stat.Start();
            }

            if (type.Equals("memory"))
            {
                Console.WriteLine("scaleout bms");
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
            StreamWriter sw = File.AppendText("api-scaleout.txt");
            sw.WriteLine(type + " " + Convert.ToString(System.DateTime.Now));
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
