using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Client
{
    public class ClientCallback : ICertificateCallback
    {
        public void NotifyClients(string msg, string expectedServer)
        {
            X509Certificate2 servCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, expectedServer);
            if (servCert != null)
            {
                if (servCert.Thumbprint == msg)
                {
                    Program.closeConnection(expectedServer); //close connection with that server
                }
            }            

            else
            {
                //check if that's my certificate
                string myName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                X509Certificate2 myCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, myName);
                if (myCert != null)
                {
                    if (myCert.Thumbprint == msg) //my certificate is compromised
                    {
                        Program.closeConnection(expectedServer);
                    }
                }
            }
        }
    }
}
