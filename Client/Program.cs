using Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Prompt();

            Console.ReadLine();
        }

        public static void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }
        public static void Prompt()
        {
            int option = 0;
            do
            {
                Console.WriteLine("1. Generate Certificate");
                Console.WriteLine("2. Add rights");
                Console.WriteLine("3. Connect to server via certificate AUTH");
                Console.WriteLine("0. EXIT");
                option = int.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        ConnectToCMS();
                        break;
                    case 2:
                        Helper.ProvideCertRight(WindowsIdentity.GetCurrent().Name);
                        break;
                    case 3:
                        ConnectToServerViaCert();
                        break;
                    case 0: //exit program
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            } while (option != 0);
        }

        private static void ConnectToCMS()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                using (WCFClient proxy = new WCFClient(binding, address))
                {
                    Console.WriteLine("Choose root: ");
                    string root = Console.ReadLine();
                    proxy.GenerateCertificate(root);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ConnectToServerViaCert()
        {
            try
            {
                NetTcpBinding bindingServer = new NetTcpBinding();
                bindingServer.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                Console.WriteLine("Expected Server(user) name: ");
                string expectedServer = Console.ReadLine();

                //gets server certificate from trustedPeople folder
                X509Certificate2 servCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, expectedServer);
                EndpointAddress addressServer = new EndpointAddress(new Uri("net.tcp://localhost:10000/WCFContracts"), new X509CertificateEndpointIdentity(servCert));

                using (WCFClientServer proxy = new WCFClientServer(bindingServer, addressServer))
                {
                    proxy.TestCommunication();
                    Console.WriteLine("TestCommunication() finished. Press <enter> to continue ...");
                    Console.ReadLine();

                    Console.WriteLine("Starting to ping server...");
                    Random r = new Random();
                    while (true)
                    {
                        Thread.Sleep(r.Next(1, 11) * 1000); //sleep 1-10s

                        proxy.PingServer(DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
