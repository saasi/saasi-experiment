using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Diagnostics;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DM.Controllers
{
    public class ScaleoutController : Controller
    {
        protected static string ScriptAgentHost = "127.0.0.1:9090";
        // GET: /<controller>/
        public IActionResult Index(string bmsGuid, int bmsCount)
        {
            Console.WriteLine(bmsGuid);
            ScaleOut("bms", bmsCount);
            WriteRecord(bmsGuid);

            return View();
        }

        public static void WriteRecord(string bmsguid) //record bms scaleout
        {
            StreamWriter sw = System.IO.File.AppendText("/data/business-scaleout.txt");
            sw.WriteLine(bmsguid?.ToString() ?? "UNKNOWN_GUID"+ " " + Convert.ToString(System.DateTime.Now));
            sw.Flush();
            sw.Dispose();
        }

        public static void ScaleOut(string type, int bmsNum)
        {
            //scalebms
            if (type.Equals("bms"))
            {
                Console.WriteLine("scaleout bms");
                RunScriptOnHost("scalebms1.sh", bmsNum);
            }

        }

        protected static void RunScriptOnHost(string ScriptFileName, int ScaleTo) {
            var _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            var url = $"http://{ScriptAgentHost}/run?script={ScriptFileName}&args[]={ScaleTo.ToString()}";
            Console.WriteLine($"Calling {url}");

            using (HttpResponseMessage response = _httpClient.GetAsync(url).Result)
            {
                using (HttpContent content = response.Content)
                {
                    string result = content.ReadAsStringAsync().Result;

                    Console.WriteLine($"StatusCode={response.StatusCode} Output={result}");
                }
            }
        }
    }

}
