using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Saasi.Shared.Workload
{
    public class IoWorkload : IWorkload
    {
        private static readonly long _fileSize = 10L * 1024L * 1024L * 1024L; //10 G

        public async Task<ExecutionResult> Run(Int64 startByte, Int64 length)
        {
            var startTime = System.DateTime.Now;
            var exceptions = false;
            String readResult = null;

            try {
                readResult = await DiskIoProcess(startByte, length);
            } catch {
                exceptions = true;
            }
            
            return new ExecutionResult {
                TaskStartedAt = startTime,
                TaskFinishedAt = System.DateTime.Now,
                HasExceptions = exceptions,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString(),
                ReadResult = readResult
            };
        }

        public async Task<String> DiskIoProcess(Int64 startByte, Int64 length)
        {
            // simulate block i/o use
            //DateTime currentTime = new DateTime();
            //currentTime = System.DateTime.Now;
            //DateTime finishTime = currentTime.AddSeconds(time);
            //String st = Guid.NewGuid().ToString();
            //String fileName = "write" + st + ".tmp";
            //FileStream fs = new FileStream(fileName, FileMode.Create);
            //fs.SetLength(_fileSize);
            //StreamWriter sw = new StreamWriter(fs);

            string randomFilePath = Directory.GetCurrentDirectory() + "\\" + "RandomStringFile" + ".txt";

            if (File.Exists(randomFilePath) == false)
            {
                return("file not found: " + randomFilePath);
            }
            FileStream fs = new FileStream(randomFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

            Int64 fileSize = fs.Length;
            if (startByte > fileSize)
            {
                return ("startByte is too big");
            }
            fs.Seek(startByte, SeekOrigin.Begin);
          
            
            int maxInt = int.MaxValue;
            byte[] readBuffer ;
            int numRead;
            Int64 numReadTotal = 0;

            StringBuilder sb = new StringBuilder();

            if ((startByte + length) > fileSize)
            {
                if ((fileSize - startByte) <= maxInt)
                {
                    readBuffer = new byte[fileSize - startByte];
                    numRead = await fs.ReadAsync(readBuffer, 0, (int)(fileSize - startByte));
                    string text = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                    sb.Append(text);
                }
                else
                {
                    readBuffer = new byte[maxInt];
                    while (((numRead = await fs.ReadAsync(readBuffer, 0, maxInt)) != 0)&&(numReadTotal+numRead) <= (fileSize - startByte))
                    {
                        string text = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                        sb.Append(text);
                        numReadTotal += numRead;
                    }
                    if (numReadTotal == (fileSize - startByte))
                    {
                        //do nothing
                    }
                    else
                    {
                        fs.Seek((numReadTotal+startByte), SeekOrigin.Begin);
                        await fs.ReadAsync(readBuffer, 0, (int)(fileSize - startByte - numReadTotal));
                        string text2 = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                        sb.Append(text2);

                    }
                    
                }
            }
            else
            {
                if (length <= maxInt)
                {
                    readBuffer = new byte[length];
                    numRead = await fs.ReadAsync(readBuffer, 0, (int)length);
                    string text = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                    sb.Append(text);
                }
                else
                {
                    readBuffer = new byte[maxInt];
                    while (((numRead = await fs.ReadAsync(readBuffer, 0, maxInt)) != 0)&& (numReadTotal+numReadTotal) <=length)
                    {
                        string text = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                        sb.Append(text);
                        numReadTotal += numRead;
                    }
                    if (numReadTotal == length)
                    {
                        //do nothing
                    }
                    else
                    {
                        fs.Seek((numReadTotal + startByte), SeekOrigin.Begin);
                        await fs.ReadAsync(readBuffer, 0, (int)(length - numReadTotal));
                        string text2 = Encoding.Unicode.GetString(readBuffer, 0, numRead);
                        sb.Append(text2);

                    }
                }
            }

            return sb.ToString();

            //while (System.DateTime.Now.CompareTo(finishTime) < 0)
            //{
            //    String s = GenerateRandomString(1000);
            //    await sw.WriteAsync(s);
            //    fs.Flush(true);
            //    // change sleep time to control block write speed
            //    //Thread.Sleep(3); 
            //}
            //sw.Dispose();

            //fs.Dispose();
            //var fi = new System.IO.FileInfo(fileName);
            //fi.Delete();
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
