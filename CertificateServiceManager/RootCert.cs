using Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
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
            try
            {
                p.Start();
            }
            catch (Exception e)
            {
                string message = String.Format("Certificate cannot be generated to {0}.Error: {1}", (Thread.CurrentPrincipal.Identity as WindowsIdentity).Name, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return;
            }

            p.WaitForExit();
            p.Dispose();

            Console.WriteLine("Created new self-signed certificate");
            
            /// try-catch necessary if either the speficied file doesn't exist or password is incorrect
            try
            {
                X509Certificate2 certificate = new X509Certificate2(root + ".cer");
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:10100/BackupData"));
                using (WCFBackupClient proxy = new WCFBackupClient(binding, address))
                {
                    proxy.ReplicateCertificate(certificate.Subject + ", thumbprint: " + certificate.Thumbprint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to replicate certificate {0}. ERROR = {1}", root, e.Message);
            }
        }
        private void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }
    }
}
