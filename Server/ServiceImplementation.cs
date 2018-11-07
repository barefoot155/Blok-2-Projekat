using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceImplementation : IWCFContracts
    {
        public void SendMessage(string msg)
        {
            //get principal
            //cert
            //Console.WriteLine(.AuthenticationType);
            IIdentity id = Thread.CurrentPrincipal.Identity; //cast as identity and get certificate
            //WindowsPrincipal.Current.
            //Console.WriteLine("Auth type {0}, Name {1}", id.AuthenticationType, id.Name); 
        }

        public void TestCommunication()
        {
            //string clientName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];
            //host.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, clientName);

            Console.WriteLine("Communication is established...");
        }
    }
}
