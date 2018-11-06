using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
   public class Helper
    {
        public static void ProvideCertRight(string user)
        {
            string userName = user.Split('\\')[1];
            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\winhttpcertcfg.exe");
            string arguments = string.Format("-g -c LOCAL_MACHINE\\My -s {0} -a {0}", userName);
            
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            info.Verb = "runas"; //run as administrator
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            p.Dispose();
        }
    }
}
