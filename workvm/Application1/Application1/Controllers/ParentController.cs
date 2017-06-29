using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;
using Application1;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Application2.Controllers
{
    public class ParentController : Controller
    {
        private Configuration ConfigSettings { get; set; }
        // GET: /<controller>/
        public ParentController(IOptions<Configuration> settings)
        {
            ConfigSettings = settings.Value;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {

            for (int i = 0; i < 4; i++)
            {
                var order = ConfigSettings.record[i].Split(' ');
                Business business = new Business(order);
                new Thread(business.Fun).Start();
            }
            
                
            return View();
        }
    }
}
