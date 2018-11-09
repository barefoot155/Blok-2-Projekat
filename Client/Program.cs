using Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
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
        static List<WCFClientServer> serverList;
        public static WCFClientServer myChannel;
        static void Main(string[] args)
        {
            serverList = new List<WCFClientServer>();
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
                Console.WriteLine("4. Revoke certificate");
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
                    case 4:
                        RevokeCertificate();
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
                var callbackInstance = new ClientCallback();
                using (WCFClient proxy = new WCFClient(callbackInstance, binding, address))
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
                var callbackInstance = new DisconnectCallback();
                using (WCFClientServer proxy = new WCFClientServer(callbackInstance, bindingServer, addressServer))
                {
                    proxy.TestCommunication();
                    // proxy.Credentials.ServiceCertificate
                    myChannel = proxy;
                    Console.WriteLine("TestCommunication() finished. Press <enter> to continue ...");
                    serverList.Add(proxy); //add to connected server list
                    Console.ReadLine();

                    Console.WriteLine("Starting to ping server...");
                    Random r = new Random();
                    while (true)
                    {
                        Thread.Sleep(r.Next(1, 15) * 1000); //sleep 1-10s

                        proxy.PingServer(DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void closeConnection(string hostname)
        {
            foreach (var item in serverList)
            {
                if(hostname ==  Helper.ExtractCommonNameFromCertificate(item.Credentials.ServiceCertificate.DefaultCertificate))
                {
                    item.Close();
                }
            }
        }

        private static void RevokeCertificate()
        {
            try
            {
                string myName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                X509Certificate2 certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, myName);
                if (certificate == null)
                    return;

                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                var callbackInstance = new ClientCallback();
                using (WCFClient proxy = new WCFClient(callbackInstance, binding, address))
                {
                    proxy.RevokeCertificate(certificate);
                    Console.WriteLine("Certificate CN={0} successfully revoked!", myName);
                }
                //remove it from installed certificates
               // CertManager.DeleteCertificateFromPersonal(certificate);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
