using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Prompt();

            WCFService host = null;
            try
            {
                host = new WCFService();
                host.OpenServer();
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

            Console.ReadKey();
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
                        default:
                            Console.WriteLine("Invalid input");
                            break;
                    }
                } while (option != 0);
            }
        }
    }
}
