using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace IO_microservice.Controllers
{
    public class HomeController : Controller
    {
        private static readonly long _fileSize = 10L * 1024L * 1024L * 1024L; //10 G

        public void ioProcess(int time)
        {
            // simulate block i/o use
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(time);
            Guid id = Guid.NewGuid();
            Console.WriteLine(id.ToString() + ":Start." + Convert.ToString(currentTime));
            String st = Guid.NewGuid().ToString();
            String fileName = "write" + st + ".tmp";
            FileStream fs = new FileStream(fileName, FileMode.Create);
            fs.SetLength(_fileSize);
            StreamWriter sw = new StreamWriter(fs);
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {


                String s = GenerateRandomString(1000);
                sw.Write(s);
                fs.Flush(true);
                // change sleep time to control block write speed
                //Thread.Sleep(3); 
            }
            sw.Dispose();

            fs.Dispose();
            var fi = new System.IO.FileInfo(fileName);
            fi.Delete();
            Console.WriteLine(id + ":Done." + Convert.ToString(System.DateTime.Now));
        }

        private static string GenerateRandomString(int length)
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
    }
}
