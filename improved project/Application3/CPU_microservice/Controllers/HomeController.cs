using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading;

namespace CPU_microservice.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult cpu(int time)
        {
            cpuProcess(time);
           // ViewData["Message"] = "CPU process finished.";
            return Content("ok");
        }

        public void cpuProcess(int time)
        {
            // simulate cpu bound operation
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Guid id = Guid.NewGuid();
            Console.WriteLine(id + ":Start." + Convert.ToString(currentTime));
            int i = 0;
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                string comparestring1 = StringDistance.GenerateRandomString(1000);
                i++;
                if (i == 50)
                {
                    Thread.Sleep(30); // Change the wait time here to adjust cpu usage.
                    i = 0;
                }
            }
            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));
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
}
