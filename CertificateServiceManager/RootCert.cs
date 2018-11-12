using Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public enum  IDType { GenerateSuccess = 0, RevokeSuccess, ReplicateSuccess, GenerateFailure, RevokeFailure, ReplicateFailure};

    public class RootCert
    {
        private String message = String.Empty;
        private EventLogEntryType evntType;
       
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
                message = String.Format("Root certificate {0} cannot be generated.Error: {1}", root, e.Message);
                evntType = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntType, Convert.ToInt32(IDType.GenerateFailure));
                return;
            }
            message = String.Format("Root certificate {0} generated.", root);
            evntType = EventLogEntryType.SuccessAudit;
            EventLogManager.WriteEntryCMS(message, evntType, Convert.ToInt32(IDType.GenerateSuccess));
            p.WaitForExit();
            p.Dispose();

            Console.WriteLine("Created new self-signed certificate");
            
            /// try-catch necessary if either the speficied file doesn't exist or password is incorrect
            try
            {
                X509Certificate2 certificate = new X509Certificate2(root + ".cer");
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri(ConfigurationSettings.AppSettings.Get("BackUp")));
                using (WCFBackupClient proxy = new WCFBackupClient(binding, address))
                {
                    message = String.Format("Root certificate {0} successfully replicated.", root);
                    evntType = EventLogEntryType.SuccessAudit;
                    EventLogManager.WriteEntryCMS(message, evntType, Convert.ToInt32(IDType.ReplicateSuccess));
                    proxy.ReplicateCertificate(certificate.Subject + ", thumbprint: " + certificate.Thumbprint);
                }
            }
            catch (Exception e)
            {
                message = String.Format("Root certificate {0} failed to replicate.Error: {1}", root, e.Message);
                evntType = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntType, Convert.ToInt32(IDType.ReplicateFailure));
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
