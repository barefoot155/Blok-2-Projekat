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
        static List<WCFClientServer> serverList;
        internal static WCFClientServer myChannel;
        static WCFClient cmsClient; //connection to CMS
        
        static void Main(string[] args)
        {
            serverList = new List<WCFClientServer>();
            
            try
            {
                Console.WriteLine("My Name: {0}", WindowsIdentity.GetCurrent().Name);
                cmsClient = ConnectToCMS();
                if(cmsClient == null)
                    throw new Exception("Connection with CMS not established");

                Console.WriteLine("*Successfully connected to CMS*");
                //WCFClientServer myChan = ConnectToServerViaCert();  //svaki treba da vrati svoj proxy, jer ovako koristi isti (static) myChannel
                //kad jedan client ugasi koenkciju sa serverom, drugima se konekcija upadne u Faulted state => ne sme static jebeni izgleda za to
                
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
                Console.WriteLine("1. Generate Certificate");
                Console.WriteLine("2. Add rights");
                Console.WriteLine("3. Connect to server via certificate AUTH");
                Console.WriteLine("4. Revoke certificate");
                Console.WriteLine("5. Ping server");
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
                         myChannel = ConnectToServerViaCert();
                        //ConnectToServerViaCert();
                        break;
                    case 4:
                        RevokeCertificate();
                        break;
                    case 5:
                        PingServer(myChannel); //u novom threadu mozda bolje
                        break;
                    case 0: //exit program
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            } while (option != 0);
        }

        private static WCFClient ConnectToCMS()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                InitializeWindowsAuthentication(binding);
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/CertificateManager"));
                var callbackInstance = new ClientCallback();
                WCFClient proxy = new WCFClient(callbackInstance, binding, address);
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
                // proxy.Credentials.ServiceCertificate
                myChannel = proxy;

                serverList.Add(proxy); //add to connected server list
                                       // Console.ReadLine();

               // PingServer(proxy);
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
                    Thread.Sleep(r.Next(1, 11) * 1000); //sleep 1-10s

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
            //foreach (var item in serverList)
            //{
            //    //var v = OperationContext.Current.Host.Credentials.ServiceCertificate.Certificate;
            //    var p = item.Endpoint.Address.Identity.IdentityClaim.Resource as X509Certificate2;
            //    //X509Certificate2 x = ((X509CertificateClaimSet)
            //    //    OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            //    var x = Helper.ExtractCommonNameFromCertificate(p);
            //    if (hostname ==  Helper.ExtractCommonNameFromCertificate(item.Credentials.ServiceCertificate.DefaultCertificate))
            //    {
            //        item.Close();
            //    }
            //}
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
