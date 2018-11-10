using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Server
{
    public class ServerCallback : ICertificateCallback
    {        
        public void NotifyClients(string msg, string serverName)
        {
            Console.WriteLine("Callback from CMS");
            X509Certificate2 servCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, serverName);
            if (servCert != null)
            {
                if (servCert.Thumbprint == msg)
                {
                    //my certificate has been compromised -> shut down server
                    Program.CloseServerConnection(serverName);
                }
            }
           
        }
    }
}
