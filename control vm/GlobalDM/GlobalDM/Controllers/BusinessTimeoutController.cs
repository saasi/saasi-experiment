using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Net.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalDM.Controllers
{
    public class BusinessTimeoutController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(Guid bmsguid)
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            Console.WriteLine(ip.ToString());
            Task.Run(async () => { await sendToDM(ip.ToString(), bmsguid.ToString()); });

            return new JsonResult(new Dictionary<string, string> { { "status", "ok" } });

        }
        static async Task sendToDM(string ip, string bmsguid)
        {

            var httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            var url = "http://" + ip + ":5002/ScaleOut?bmsguid=" + bmsguid;
            var response = await httpClient.GetAsync(url);
            Console.WriteLine("sent:" + url);
        }
    }


}
