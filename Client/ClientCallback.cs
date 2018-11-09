using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class ClientCallback : ICertificateCallback
    {
        public void NotifyClients(string msg, string expectedServer)
        {
            X509Certificate2 servCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, expectedServer);
            if (servCert.Thumbprint == msg)
            {
                Program.closeConnection(expectedServer);
            }
        }
    }
}
