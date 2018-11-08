using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        /// <summary>
        /// Delete everything after comma -> leave only CN="username"
        /// </summary>
        /// <param name="clientCert"></param>
        /// <returns></returns>
        public static string ExtractCommonNameFromCertificate(X509Certificate2 clientCert)
        {
            int commaIndex = clientCert.SubjectName.Name.IndexOf(',');
            string commonName = clientCert.SubjectName.Name.Remove(commaIndex); //CN=username

            return commonName.Substring(3); //username
        }

        
    }
}
