using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading;

namespace CertificateServiceManager
{
    public class CertificateManager : ICertificateManager
    {
        public X509Certificate2 GenerateCertificate(string root)
        {
            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\makecert.exe");
            string userName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];
            string arguments = string.Format("-sv {0}.pvk -iv {1}.pvk -n \"CN = {0}\" -pe -ic {1}.cer {0}.cer -sr localmachine -ss My -sky exchange", userName, root);
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            p.Dispose();

            //create .pfx file
            Process p2 = new Process();
            path = (AppDomain.CurrentDomain.BaseDirectory + @"\pvk2pfx.exe"); 
            arguments = string.Format("/pvk {0}.pvk /pi 123 /spc {0}.cer /pfx {0}.pfx", userName);
            info = new ProcessStartInfo(path, arguments);
            p2.StartInfo = info;
            p2.Start();
            p2.WaitForExit();

            p2.Dispose();
            return null;
        }

        public void RevokeCertificate()
        {
            throw new NotImplementedException();
        }
    }
}
