using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificateServiceManager
{
    public class RootCert
    {
        public void createRootCertificate(string root)
        {
            if (File.Exists(root + ".cer"))
            {
                Console.WriteLine("Self-signed certificate <{0}> already exists", root);
                return;
            }

            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\makecert.exe");
            string arguments = string.Format("-n \"CN = {0}\" -r -sv {0}.pvk {0}.cer", root);
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            p.Dispose();

            Console.WriteLine("Created new self-signed certificate");
        }

        
    }
}
