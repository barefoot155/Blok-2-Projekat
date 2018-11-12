using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.ServiceModel;
using System.Security.Principal;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.ServiceModel.Description;
using System.Configuration;

namespace Server
{
    public class WCFService
    {
        ServiceHost host;
        public WCFService()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            string address = ConfigurationSettings.AppSettings.Get("ServerHost");
            host = new ServiceHost(typeof(ServiceImplementation));
            SpecifyAuditingBehavior(host);
            host.AddServiceEndpoint(typeof(IWCFContracts), binding, address);

            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;

            ///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            string srvName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            //host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromFile(filename);
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvName);

            //
            
        }

        public void SpecifyAuditingBehavior(ServiceHost host)
        {
            ServiceSecurityAuditBehavior audit = new ServiceSecurityAuditBehavior();
            audit.AuditLogLocation = AuditLogLocation.Application;
            host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            host.Description.Behaviors.Add(audit);
        }

        public void OpenServer()
        {
            host.Open();
        }     
        public void CloseServer()
        {
            host.Close();
        }
    }
}
