using Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Prompt();

            NetTcpBinding bindingServer = new NetTcpBinding();
            bindingServer.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            
            string filename = WindowsIdentity.GetCurrent().Name.Split('\\')[1] + ".pfx";
            // X509Certificate2 clientCert = CertManager.GetCertificateFromFile(filename);

            //gets server certificate
            X509Certificate2 servCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "user2");
            EndpointAddress addressServer = new EndpointAddress(new Uri("net.tcp://localhost:10000/WCFContracts"), new X509CertificateEndpointIdentity(servCert));

            using (WCFClientServer proxy = new WCFClientServer(bindingServer, addressServer))
            {
                proxy.TestCommunication();
                Console.WriteLine("TestCommunication() finished. Press <enter> to continue ...");
                Console.ReadLine();
            }
        }

        public static void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }
        public static void Prompt()
        {
            
            
            NetTcpBinding binding = new NetTcpBinding();
            InitializeWindowsAuthentication(binding);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));

            using (WCFClient proxy = new WCFClient(binding, address))
            {
                int option = 0;
                do
                {
                    Console.WriteLine("1. Generate Certificate");
                    Console.WriteLine("2. Add rights");
                    Console.WriteLine("0. EXIT");
                    option = int.Parse(Console.ReadLine());

                    switch (option)
                    {
                        case 1:
                            Console.WriteLine("Choose root: ");
                            string root = Console.ReadLine();
                            proxy.GenerateCertificate(root);
                            break;
                        case 2:
                            Helper.ProvideCertRight(WindowsIdentity.GetCurrent().Name);
                            break;
                        case 0:
                            Console.WriteLine("Prompt terminated.");
                            break;
                        default:
                            Console.WriteLine("Invalid input");
                            break;
                    }
                } while (option != 0);
            }
        }
        
    }
}
