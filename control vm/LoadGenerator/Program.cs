using System;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoadGenerator.MockUsers;

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
            for (int i =1; i<=userCount;++i) {
                var t = new Thread(() => RunUser1(duration));
                t.Start();
                Console.Error.WriteLine($"Started Thread #{i}");
                users.Add(t);
                Thread.Sleep(200); // delay a little bit before creating the next thread
                                    // avoid all threads generating requests at the same interval;
            }
        }
        static void RunUser1(int duration=1){
            Console.WriteLine("RUN!");
            IApplicationUser user = new Application1User();
            var t =Task.Run(async()=> {await user.Run("http://localhost:5004", duration);});
            t.Wait();
        }


        /* EVALUATION 3 */
        static void RunLoad3(int userCount=1,int duration =1) {
            List<Thread> users = new List<Thread>();
            for (int i =1; i<=userCount;++i) {
                var t = new Thread(() => RunUser3(duration));
                t.Start();
                Console.Error.WriteLine($"Started Thread #{i}");
                users.Add(t);
                Thread.Sleep(200); // delay a little bit before creating the next thread
                                    // avoid all threads generating requests at the same interval;
            }
        }
        static void RunUser3(int duration = 1){
            Console.WriteLine("RUN!");
            IApplicationUser user = new Application3User();
            var t =Task.Run(async()=> {await user.Run("http://localhost:5000", duration);});
            t.Wait();
        }

    }
}
