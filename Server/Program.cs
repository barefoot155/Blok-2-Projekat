using Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        static CMSClient cmsClient; //connection to CMS

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
                Console.WriteLine("1. Generate Certificate from CA (with private key)");
                Console.WriteLine("2. Generate Certificate from CA (without private key)");
                Console.WriteLine("3. Add rights to certificate");
                Console.WriteLine("4. Host Server with certificate AUTH");
                Console.WriteLine("5. Revoke certificate");
                Console.WriteLine("6. EXIT");
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
                        HostServer();
                        break;
                    case 5:
                        RevokeCertificate();
                        break;
                    case 6: //exit program
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            } while (option != 6);
        }

        public static void CloseServerConnection(string serverName)
        {
            if (myHost != null)
                myHost.CloseServer();
        }

        private static CMSClient ConnectToCMS()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri(ConfigurationSettings.AppSettings.Get("CMSProxy")));
                var callbackInstance = new ServerCallback();

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
                EndpointAddress address = new EndpointAddress(new Uri(ConfigurationSettings.AppSettings.Get("CMSProxy")));
                var callbackInstance = new ServerCallback();
                cmsClient.RevokeCertificate(certificate);
                Console.WriteLine("Certificate CN={0} successfully revoked!", myName);
                //remove it from installed certificates
                //CertManager.DeleteCertificateFromPersonal(certificate);


                //ZAKOMENTARISANO ZBOG LAKSEG TESTIRANJA -> OTKOMENTARISATI NA KRAJU             <----------------- OTKOMENTARISATI NA KRAJU
                //Console.WriteLine("Generating new certificate...");
                //Console.WriteLine("Enter root name: ");
                //cmsClient.GenerateCertificate(Console.ReadLine());                      
                //}


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
