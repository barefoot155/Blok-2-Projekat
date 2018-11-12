using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;
using System.Security;
using System.IO;
using System.ServiceModel;

namespace CertificateServiceManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class CertificateManager : ICertificateManager
    {
        private string message = string.Empty;
        private static List<ICertificateCallback> clients = new List<ICertificateCallback>();
        public CertificateManager()
        {
        }

        public void GenerateCertificate(string root)
        {
            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\makecert.exe");
            string userName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];

            //get groups from windowsIdentity.Groups
            string groups = GetUserGroups((Thread.CurrentPrincipal.Identity as WindowsIdentity));

            string arguments = string.Format("-sv {0}.pvk -iv {1}.pvk -n \"CN = {0},OU={2}\" -pe -ic {1}.cer {0}.cer -sr localmachine -ss My -sky exchange", userName, root, groups);
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            p.StartInfo = info;
            try
            {
                p.Start();
            }
            catch(Exception e)
            {
                message = String.Format("Certificate cannot be generated to {0}.Error: {1}", userName, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return;
            }
            p.WaitForExit();
            p.Dispose();

            
            //create .pfx file
            Process p2 = new Process();
            path = (AppDomain.CurrentDomain.BaseDirectory + @"\pvk2pfx.exe");
            //Console.WriteLine("Enter private key: ");
            //string pvk = Console.ReadLine();
            arguments = string.Format("/pvk {0}.pvk /pi {1} /spc {0}.cer /pfx {0}.pfx", userName, "123");
            info = new ProcessStartInfo(path, arguments);
            p2.StartInfo = info;
            try
            {
                p2.Start();
            }
            catch(Exception e)
            {
                message = String.Format("Certificate cannot be generated to {0}.Error: {1}", userName, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return;
            }
            p2.WaitForExit();
            p2.Dispose();

            message = String.Format("Certificate generated to {0}.", (Thread.CurrentPrincipal.Identity as WindowsIdentity).Name);
            EventLogEntryType evntTypeSuccess = EventLogEntryType.SuccessAudit;
            EventLogManager.WriteEntryCMS(message, evntTypeSuccess);

            Replicate(userName, "123");
        }

        public void GenerateCertificateWithoutPVK(string root)
        {
            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\makecert.exe");
            string userName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];

            //get groups from windowsIdentity.Groups
            string groups = GetUserGroups((Thread.CurrentPrincipal.Identity as WindowsIdentity));

            string arguments = string.Format("-iv {1}.pvk -n \"CN = {0},OU={2}\" -ic {1}.cer {0}.cer -sr localmachine -ss My -sky exchange", userName, root, groups);
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            p.StartInfo = info;
            try
            {
                p.Start();
            }
            catch (Exception e)
            {
                message = String.Format("Certificate cannot be generated to {0}.Error: {1}", userName, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return;
            }
            p.WaitForExit();
            p.Dispose();

            Replicate(userName, "");
        }

        private void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }

        private string GetUserGroups(WindowsIdentity windowsIdentity)
        {
            string groups = "";
            foreach (IdentityReference group in windowsIdentity.Groups)
            {
                SecurityIdentifier sid =
                (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount)).ToString();
                if (name.Contains('\\'))
                    name = name.Split('\\')[1]; 
                if (name == "RegionWest" || name == "RegionEast" || name == "RegionNorth" || name == "RegionSouth")
                {
                    if (groups != "")
                        groups += "_" + name;
                    else
                        groups = name;
                }
            }

            return groups;
        }
        
        public void RevokeCertificate(X509Certificate2 cert)
        {
            if (cert == null)
                throw new ArgumentNullException("cert", "Certificate cannot be null");

            AddToRevocationList(cert);
            //DeleteLocalCertificate(cert);   //ZAKOMENTARISANO ZA SVRHE TESTIRANJA (da ne brise svaki put) -> otkomentarisati na kraju

            
            clients.Remove(OperationContext.Current.GetCallbackChannel<ICertificateCallback>());
            NotifyAllClients(cert);
        }

        private void NotifyAllClients(X509Certificate2 cert)
        {
            foreach (var item in clients)
            {
                try
                {
                    item.NotifyClients(cert.Thumbprint, Helper.ExtractCommonNameFromCertificate(cert));
                }
                catch (Exception) //some clients might have disconnected in the meantime
                { }
            }
        }

        /// <summary>
        /// Deletes .cer .pvk and .pfx files from local CMS/Debug folder
        /// </summary>
        /// <param name="cert"></param>
        private void DeleteLocalCertificate(X509Certificate2 cert)
        {
            string certName = Helper.ExtractCommonNameFromCertificate(cert);
            string[] certFiles = { certName + ".cer", certName + ".pvk", certName + ".pfx" };

            foreach(string file in certFiles)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
                
        }

        private void AddToRevocationList(X509Certificate2 cert)
        {
            using (StreamWriter sw = new StreamWriter("RevocationList.txt", true))
            {                
                sw.WriteLine(cert.Thumbprint);
            }
            NetTcpBinding binding = new NetTcpBinding();
            InitializeWindowsAuthentication(binding);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:10100/BackupData"));
            try
            {
                using (WCFBackupClient proxy = new WCFBackupClient(binding, address))
                {
                    proxy.ReplicateRevokedCert(cert.Subject + ", thumbprint: " + cert.Thumbprint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to replicate certificate {0}. ERROR = {1}", cert.Subject, e.Message);
            }
        }

        /// <summary>
        /// adds CMS client to the list of all clients
        /// </summary>
        public void RegisterClient()
        {
            ICertificateCallback callback = OperationContext.Current.GetCallbackChannel<ICertificateCallback>();
            if (!clients.Contains(callback))
                clients.Add(callback);
        }

        /// <summary>
        /// Replicates generated certificate 
        /// </summary>
        /// <param name="userName">Certificate file name</param>
        /// <param name="password">Password for .pvk file</param>
        private void Replicate(string userName, string password)
        {
            /// try-catch necessary if either the speficied file doesn't exist or password is incorrect
            try
            {
                X509Certificate2 certificate;
                if (password == "")
                    certificate = new X509Certificate2(userName + ".cer");
                else
                    certificate = new X509Certificate2(userName + ".cer", password);

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
                Console.WriteLine("Error while trying to replicate certificate {0}. ERROR = {1}", userName, e.Message);
            }
        }
    }
}
