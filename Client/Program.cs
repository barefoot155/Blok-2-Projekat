using Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Claims;
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
        //static List<WCFClientServer> serverList;
        internal static WCFClientServer myChannel;
        static CMSClient cmsClient; //connection to CMS
        
        static void Main(string[] args)
        {
            //serverList = new List<WCFClientServer>();
            
            try
            {
                Console.WriteLine("My Name: {0}", WindowsIdentity.GetCurrent().Name);
                cmsClient = ConnectToCMS();
                if(cmsClient == null)
                    throw new Exception("Connection with CMS not established");

                Console.WriteLine("*Successfully connected to CMS*");
                                
                Prompt();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (cmsClient != null)
                    cmsClient.Close();
                if (myChannel != null)
                    myChannel.Close();
            }

            Console.WriteLine("\n> Press enter to close program");
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
                Console.WriteLine("1. Generate Certificate (with private key)");
                Console.WriteLine("2. Generate Certificate (without private key)");
                Console.WriteLine("3. Add rights");
                Console.WriteLine("4. Connect to server via certificate AUTH");
                Console.WriteLine("5. Revoke certificate");
                Console.WriteLine("6. Ping server");
                Console.WriteLine("7. EXIT");
                int.TryParse(Console.ReadLine(),out option);

                switch (option)
                {
                    case 1:
                        Console.WriteLine("Choose root: ");
                        string root = Console.ReadLine();
                        cmsClient.GenerateCertificate(root);
                        break;
                    case 2:
                        Console.WriteLine("Choose root: ");
                        string root2 = Console.ReadLine();
                        cmsClient.GenerateCertificateWithoutPVK(root2);
                        break;
                    case 3:
                        Helper.ProvideCertRight(WindowsIdentity.GetCurrent().Name);
                        break;
                    case 4:
                         myChannel = ConnectToServerViaCert();
                        //ConnectToServerViaCert();
                        break;
                    case 5:
                        RevokeCertificate();
                        break;
                    case 6:
                        PingServer(myChannel); //u novom threadu mozda bolje
                        break;
                    case 7: //exit program
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            } while (option != 7);
        }

        private static CMSClient ConnectToCMS()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                var callbackInstance = new ClientCallback();
                CMSClient proxy = new CMSClient(callbackInstance, binding, address);
                proxy.RegisterClient();

                return proxy;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static WCFClientServer ConnectToServerViaCert()
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
                WCFClientServer proxy = new WCFClientServer(callbackInstance, bindingServer, addressServer);

                proxy.TestCommunication();
                Console.WriteLine("TestCommunication() with server successful.");

                return proxy;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static void PingServer(WCFClientServer proxy)
        {
            Console.WriteLine("Starting to ping server...");
            Random r = new Random();
            try
            {
                while (true)
                {
                    Thread.Sleep(r.Next(1, 10) * 1000); //sleep 1-10s

                    proxy.PingServer(DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public static void closeConnection(string hostname)
        {
            if (myChannel.State == CommunicationState.Opened)
                myChannel.Close();
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
                cmsClient.RevokeCertificate(certificate);
                Console.WriteLine("Certificate CN={0} successfully revoked!", myName);
                //}
                //remove it from installed certificates
                // CertManager.DeleteCertificateFromPersonal(certificate);

                //ZAKOMENTARISANO ZBOG LAKSEG TESTIRANJA -> OTKOMENTARISATI NA KRAJU             <----------------- OTKOMENTARISATI NA KRAJU
            //    Console.WriteLine("Generating new certificate...");
            //    Console.WriteLine("Enter root name: ");
            //    cmsClient.GenerateCertificate(Console.ReadLine());
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
