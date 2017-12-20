using System;

using System.IO;

using System.Text;
using System.Threading;

namespace BusinessFunction
{
    public class io
    {
        private  int timetorun;
        private static readonly long _fileSize = 10L * 1024L * 1024L * 1024L; //10 G
        public io(String timetorun)
        {
            this.timetorun = Convert.ToInt16(timetorun);
        }

        public  void Fun(object state)
        {
            DateTime currentTime = new DateTime();
            currentTime = System.DateTime.Now;
            DateTime finishTime = currentTime.AddSeconds(timetorun);
            Console.WriteLine("IO Start." + Convert.ToString(currentTime));
            String st = Guid.NewGuid().ToString();
            String fileName = "write" + st + ".tmp";
            FileStream fs = new FileStream(fileName, FileMode.Create);
            fs.SetLength(_fileSize);
            StreamWriter sw = new StreamWriter(fs);
            while (System.DateTime.Now.CompareTo(finishTime) < 0)
            {


                String s = io.GenerateRandomString(1000);
                sw.Write(s);
                fs.Flush(true);
                //Thread.Sleep(1);
            }
            sw.Dispose();

            fs.Dispose();
            var fi = new System.IO.FileInfo(fileName);
            fi.Delete();
            Console.WriteLine( "IO Done." + Convert.ToString(System.DateTime.Now));
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
