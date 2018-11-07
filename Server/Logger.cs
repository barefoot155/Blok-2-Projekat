using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    static class Logger
    {
        static int ID;
        static string logFile;
        static readonly object dummy;

        static Logger() //static constructor
        {
            ID = 0;
            logFile = "serverLog.txt";
            dummy = new object();

            if (File.Exists(logFile)) //existing file -> set ID to last entered ID
            {
                string lastLine = File.ReadLines(logFile).Last();
                int index = lastLine.IndexOf('>');
                int lastID = int.Parse(lastLine.Substring(1, index - 1));

                ID = lastID;
            }
        }
        

        public static void LogData(DateTime dt, string CommonName)
        {
            using (StreamWriter sw = new StreamWriter(logFile, true)) //append data to (existing) file
            {
                lock (dummy) //multithreading safety (multiple clients pinging the same server simultaneously)
                {
                    sw.WriteLine("<{0}>: <{1}>; <{2}>", ++ID, dt.ToLongTimeString(), CommonName);
                }
            }
            Console.WriteLine("Client <{0}> ping logged", CommonName);
        }
    }
}
