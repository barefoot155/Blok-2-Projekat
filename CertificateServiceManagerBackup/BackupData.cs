using Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateServiceManagerBackup
{
    public class BackupData : IBackupData
    {
        //public List<string> RevocationList = new List<string>();
        //public List<string> CertificatesList = new List<string>();
        static readonly object dummy = new object();

        public void ReplicateCertificate(string cert)
        {
            //CertificatesList.Add(cert);
            using (StreamWriter sw = new StreamWriter("CertListBackup.txt", true))
            {
                lock (dummy)
                {
                    sw.WriteLine(cert);
                }
            }
            Console.WriteLine("Data replicated...");
        }

        public void ReplicateRevokedCert(string cert)
        {
            //RevocationList.Add(cert);
            using (StreamWriter sw = new StreamWriter("RevocationListBackup.txt", true))
            {
                lock (dummy)
                {
                    sw.WriteLine(cert);
                }
            }
            Console.WriteLine("Data replicated...");
        }
    }
}
