using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Contract;
using System.ServiceModel.Description;

namespace CertificateServiceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            InitializeWindowsAuthentication(binding);

            ServiceHost host = new ServiceHost(typeof(CertificateManager));
            SpecifyAuditingBehavior(host);
            if (ServerHosting(host, binding))
            {
                Console.WriteLine("WCFService is started...");
                RootCert rc = new RootCert();
                Console.WriteLine("Enter root name: ");
                string root = Console.ReadLine();
                rc.createRootCertificate(root);
                Console.WriteLine("Press <enter> to close server...");                
            }

            Console.ReadLine();

            host.Close();
        }

        public static void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }

        public static bool ServerHosting(ServiceHost host, NetTcpBinding binding)
        {
            string address = "net.tcp://localhost:9999/CertificateManager";            
            host.AddServiceEndpoint(typeof(ICertificateManager), binding, address);

            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            try
            {
                host.Open();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
                return false;
            }  
        }

        public static void SpecifyAuditingBehavior(ServiceHost host)
        {
            ServiceSecurityAuditBehavior audit = new ServiceSecurityAuditBehavior();
            audit.AuditLogLocation = AuditLogLocation.Application;
            host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            host.Description.Behaviors.Add(audit);
        }
    }
}
