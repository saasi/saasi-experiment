using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ioSpace
{
    public class io
    {
        private static String Timeout;
        public io(String timeout)
        {
            Timeout = timeout;
        }

        public  void Fun()
        {
            String st = Guid.NewGuid().ToString();
            FileStream fs = new FileStream("write" + Convert.ToString(st) + ".tmp", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(Convert.ToInt16(Timeout));
            Console.WriteLine("IO service start." + Convert.ToString(currentTime));
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {
                String s = io.GenerateRandomString(800);
                sw.Write(s);
            }

            fs.Flush();
            fs.Dispose();
            Console.WriteLine("IO service end." + Convert.ToString(System.DateTime.Now));
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
