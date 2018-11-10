using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static WCFService myHost; //host service for my Clients
        static WCFClient cmsClient; //connection to CMS

        internal static List<IDisconnectCallback> myClients = new List<IDisconnectCallback>(); //list of servers clients
        static void Main(string[] args)
        {
            //connect to CMS on start-up
            try
            {
                EventLogManager.InitializeServerEventLog();

                cmsClient = ConnectToCMS();
                if (cmsClient == null)
                    throw new Exception("Connection with CMS not established");

                Console.WriteLine("*Successfully connected to CMS*");
                Prompt();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cmsClient.Close();
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
                Console.WriteLine("1. Generate Certificate from CA");
                Console.WriteLine("2. Add rights to certificate");
                Console.WriteLine("3. Host Server with certificate AUTH");
                Console.WriteLine("4. Revoke certificate");
                Console.WriteLine("0. EXIT");
                option = int.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        Console.WriteLine("Choose root: ");
                        string root = Console.ReadLine();
                        cmsClient.GenerateCertificate(root);
                        break;
                    case 2:
                        Helper.ProvideCertRight(WindowsIdentity.GetCurrent().Name);
                        break;
                    case 3:
                        HostServer();
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

        public static void CloseServerConnection(string serverName)
        {
            if (myHost != null)
                myHost.CloseServer();
        }

        private static WCFClient ConnectToCMS()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                var callbackInstance = new ServerCallback();

                WCFClient proxy = new WCFClient(callbackInstance, binding, address);
                proxy.RegisterClient();
                
                //proxy.GenerateCertificate(root);
                return proxy;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }            
        }

        private static void HostServer()
        {

            WCFService host = null;
            try
            {
                host = new WCFService();
                host.OpenServer();
                myHost = host;
                Console.WriteLine("WCFService is started.\nPress <enter> to stop ...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                host.CloseServer();
            }
        }

        private static void RevokeCertificate()
        {
            try
            {
                string myName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                X509Certificate2 certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, myName);

                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                var callbackInstance = new ServerCallback();
                using (WCFClient proxy = new WCFClient(callbackInstance, binding, address))
                {
                    proxy.RevokeCertificate(certificate);
                    Console.WriteLine("Certificate CN={0} successfully revoked!", myName);
                    //remove it from installed certificates
                    CertManager.DeleteCertificateFromPersonal(certificate);
                    Console.WriteLine("Installing new certificate...");
                    Console.WriteLine("Enter root name: ");
                    proxy.GenerateCertificate(Console.ReadLine()); 
                }
                
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
