using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saasi.Shared.Workload
{
    class WriteFiles
    {
        public static string GenerateRandomStringFile(Int64 length)
        {
            var r = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            for (Int64 i = 0; i < length; i++)
            {
                int c = r.Next(97, 123);
                sb.Append(Char.ConvertFromUtf32(c));
            }
            //If not exist, create a random string file
            string randomFilePath = Directory.GetCurrentDirectory() + "\\" + "RandomStringFile" + ".txt";
            if (File.Exists(randomFilePath))
                File.Delete(randomFilePath);
            FileStream fs = new FileStream(randomFilePath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            // if it is needed to be asynchronously?
            sw.Write(sb);

            fs.Flush();
            sw.Dispose();
            fs.Dispose();
            return "successfully writen";
        }
    }
}
