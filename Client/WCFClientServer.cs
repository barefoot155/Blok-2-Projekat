using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class WCFClientServer : ChannelFactory<IWCFContracts>, IWCFContracts, IDisposable
    {
        IWCFContracts proxy;
        public WCFClientServer(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            //dodati klijentov sertifikat
            string clientName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, clientName);

            proxy = this.CreateChannel();
        }

        public void SendMessage(string msg)
        {
            try
            {
                proxy.SendMessage(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public void TestCommunication()
        {
            try
            {
                proxy.TestCommunication();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
