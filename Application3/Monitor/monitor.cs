using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Monitor
{
    class monitor
    {
        private static double IO_LIMIT;
        private static double CPU_LIMIT;
        private static double MEMORY_LIMIT;

        public static void Main(string[] args)
        {
            monitor dm = new monitor();
            new Thread(monitorBusiness).Start();
            while (true)
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
                            if (dm.getUsage(container) > IO_LIMIT)
                            {
                                reportGM("IO");
                                recordInSql(container.Value, System.DateTime.Now);
                            }
                                
                        if (container.Value.Equals("CPU"))
                            if (dm.getUsage(container) > CPU_LIMIT)
                            {
                                reportGM("CPU");
                                recordInSql(container.Value, System.DateTime.Now);
                            }
                               
                        if (container.Value.Equals("MEMORY"))
                            if (dm.getUsage(container) > CPU_LIMIT)
                            {
                                reportGM("MEMORY");
                                recordInSql(container.Value, System.DateTime.Now);
                            }
                                
                    }
                }

                
            }
            

        }

        public static void monitorBusiness() //A thread to listen message from business
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "report_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    int businessId = Convert.ToInt16(message.Split(' ')[0]);
                    addTimeoutTimes(businessId); // add timeout times in database
                    if (getTimeoutTimes(businessId) >= 3) // get the business timeout times from database
                    {
                        reportGM("");
                        recordInSql(businessId, System.DateTime.Now); //write scale out record to database
                    }
                };
            }
        }
            public double getUsage(KeyValuePair<string, string> container)
        {
            //get usage of the container
            return 0;
        }


        public Dictionary<string, string> getContainerList()
        {
            //call docker api to get each container's image
            return new Dictionary<string, string> ();
        }

        public static void reportGM(string type)
        {
            // report microservice type that needs to scale out
        }

        public static void writeToSql(int businessId) //
        {

        }
        public static void addTimeoutTimes(int businessId)// add timeout times in database
        {

        }

        public static int getTimeoutTimes(int businessId)
        {
            return 0;
        }

        public static void recordInSql(int businessId, DateTime writeTime)
        {

        }

        public static void recordInSql(string type, DateTime writeTime)
        {

        }
    }
}