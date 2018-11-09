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

namespace CertificateServiceManager
{
    public class CertificateManager : ICertificateManager
    {
        //private EventLog generateCertLog = new EventLog();
        private string message = string.Empty;

        public X509Certificate2 GenerateCertificate(string root)
        {
            //if(!EventLog.SourceExists("ProjectEvents")) //ako baca exception potrebno je u regedit dodeli full control prava korisniku koji pokrece CertificateServiceManager
            //{
            //    EventLog.CreateEventSource("ProjectEvents", "CertificateLog");
            //}
            //generateCertLog.Source = "ProjectEvents";
            //generateCertLog.Log = "CertificateLog";

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
                message = String.Format("Certificate cannot be generated to {0}.Error: {1}", (Thread.CurrentPrincipal.Identity as WindowsIdentity).Name, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                //CaptureEvent(message, evntTypeFailure);
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return null;
            }
            p.WaitForExit();
            p.Dispose();

            
            //create .pfx file
            Process p2 = new Process();
            path = (AppDomain.CurrentDomain.BaseDirectory + @"\pvk2pfx.exe");
            arguments = string.Format("/pvk {0}.pvk /pi 123 /spc {0}.cer /pfx {0}.pfx", userName);
            info = new ProcessStartInfo(path, arguments);
            p2.StartInfo = info;
            try
            {
                p2.Start();
            }
            catch(Exception e)
            {
                message = String.Format("Certificate cannot be generated to {0}.Error: {1}", (Thread.CurrentPrincipal.Identity as WindowsIdentity).Name, e.Message);
                EventLogEntryType evntTypeFailure = EventLogEntryType.FailureAudit;
                //CaptureEvent(message, evntTypeFailure);
                EventLogManager.WriteEntryCMS(message, evntTypeFailure);
                return null;
            }
            p2.WaitForExit();
            p2.Dispose();

            message = String.Format("Certificate generated to {0}.", (Thread.CurrentPrincipal.Identity as WindowsIdentity).Name);
            EventLogEntryType evntTypeSuccess = EventLogEntryType.SuccessAudit;
            //CaptureEvent(message, evntTypeSuccess);
            EventLogManager.WriteEntryCMS(message, evntTypeSuccess);

            return null;
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

        //private void CaptureEvent(string message, EventLogEntryType evntType)
        //{           
        //    generateCertLog.WriteEntry(message, EventLogEntryType.SuccessAudit);
        //}

        public void RevokeCertificate(X509Certificate2 cert)
        {
            if (cert == null)
                throw new ArgumentNullException("cert", "Certificate cannot be null");

            //if(!cert.HasPrivateKey) //klijent koji zahteva povlacenje sertifikata NEMA privatni kljuc
            //{
            //    throw new SecurityException("Private key is needed to revoke certificate");
            //}

            AddToRevocationList(cert);
            DeleteLocalCertificate(cert);
            //NotifyAllClients(cert);
        }

        private void NotifyAllClients(X509Certificate2 cert)
        {
            throw new NotImplementedException();
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
        }
    }
}
