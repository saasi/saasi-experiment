using System;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoadGenerator.MockUsers;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;

namespace LoadGenerator
{
    class Program
    {
        static void Main(string[] args)
        { 
            if (args.Length!=3) {
                Console.WriteLine("Usage:");
                Console.WriteLine("  LoadGenerator <type> <usercount> <duration>");
                return;
            }
            resetDataAsync();
            var type = int.Parse(args[0]);
            var userCount = int.Parse(args[1]);
            var duration = int.Parse(args[2]);
            switch (type) {
                case 1:
                    Console.Error.WriteLine("========================== EVALUATION 1 =============================");
                    Console.WriteLine($"Concurrent Users: {userCount}");
                    Console.WriteLine($"Duration: {duration} seconds");
                    RunLoad1(userCount,duration);
                    break;
                case 2:
                    Console.Error.WriteLine("========================== EVALUATION 2 =============================");
                    Console.WriteLine($"Concurrent Users: {userCount}");
                    Console.WriteLine($"Duration: {duration} seconds");
                    RunLoad3(userCount,duration); // the same as 3
                    break;
                case 3:
                    Console.Error.WriteLine("========================== EVALUATION 3 =============================");
                    Console.WriteLine($"Concurrent Users: {userCount}");
                    Console.WriteLine($"Duration: {duration} seconds");
                    RunLoad3(userCount,duration);
                    break;
                default:
                    Console.Error.WriteLine("Evaluation number must be 1-3");
                    break;                    
            }
        }
        /* EVALUATION 1 */
        static void RunLoad1(int userCount=1, int duration =1) {
            List<Thread> users = new List<Thread>();
            for (int j = 0; j < duration; j++)
            {
                for (int i = 1; i <= userCount / duration; ++i)
                {
                    var t = new Thread(() => RunUser1(duration));
                    t.Start();
                    Console.Error.WriteLine($"Started Thread #{i}");
                    users.Add(t);
                    Thread.Sleep(200); // delay a little bit before creating the next thread                                       // avoid all threads generating requests at the same interval;
                }
                StreamWriter sw = System.IO.File.AppendText("request-time.txt");
                sw.WriteLine( $"{j + 1}: {Convert.ToString(System.DateTime.Now)}");
                sw.Flush();
                sw.Dispose();
                Thread.Sleep(5 * 1000 * userCount / duration / 4);
            }
        }
        static void RunUser1(int duration=1){
            Console.WriteLine("RUN!");
            IApplicationUser user = new Application1User();
            var t =Task.Run(async()=> {await user.Run("http://10.137.0.81:5000", duration);});
            t.Wait();
        }


        /* EVALUATION 3 */
        static void RunLoad3(int userCount=1,int duration =1) {
            List<Thread> users = new List<Thread>();
            for (int j = 0; j < duration; j++)
            {
                for (int i = 1; i <= userCount / duration; ++i)
                {
                    var t = new Thread(() => RunUser3(duration));
                    t.Start();
                    Console.Error.WriteLine($"Started Thread #{i}");
                    users.Add(t);
                    Thread.Sleep(200); // delay a little bit before creating the next thread                                       // avoid all threads generating requests at the same interval;
                }
                StreamWriter sw = System.IO.File.AppendText("request-time.txt");
                sw.WriteLine( $"{j + 1}: {Convert.ToString(System.DateTime.Now)}");
                sw.Flush();
                sw.Dispose();
                Thread.Sleep(5 * 1000 * userCount / duration);
            }

        }
        static void RunUser3(int duration = 1){
            Console.WriteLine("RUN!");
            Application3User user = new Application3User();
            var t = Task.Run(async ()=>await user.Run("http://localhost:5000", duration));
            t.Wait(); // prevents the program from existing before this thread finishes
        }

        private static async Task resetDataAsync()
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            var url = new Uri("http://10.137.0.81:8080/globalMonitor/resetData");
            try
            {
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine("{response.StatusCode}");
                //Console.WriteLine($"User {_guid} {DateTime.Now.ToString()} {timestart}");
            }
            catch
            {
                Console.WriteLine("Network Error");
            }
        }

    }
}
