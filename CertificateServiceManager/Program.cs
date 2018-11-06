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
            string root = "TestCA";

            NetTcpBinding binding = new NetTcpBinding();
            InitializeWindowsAuthentication(binding);

            ServiceHost host = new ServiceHost(typeof(CertificateManager));
            if (ServerHosting(host, binding))
            {
                Console.WriteLine("WCFService is started.\nPress <enter> to stop ...");
                RootCert rc = new RootCert();
                rc.createRootCertificate(root);
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
    }
}
