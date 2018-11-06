using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceImplementation : IWCFContracts
    {
        public void SendMessage(string msg, byte[] sign)
        {
            throw new NotImplementedException();
        }

        public void TestCommunication()
        {
            //string clientName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];
            //host.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, clientName);

            Console.WriteLine("Communication is established...");
        }
    }
}
