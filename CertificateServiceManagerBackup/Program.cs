using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace CertificateServiceManagerBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            InitializeWindowsAuthentication(binding);
            string address = "net.tcp://localhost:10100/BackupData";

            ServiceHost hostBackup = new ServiceHost(typeof(BackupData));
            hostBackup.AddServiceEndpoint(typeof(IBackupData), binding, address);

            hostBackup.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            hostBackup.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            try
            {
                hostBackup.Open();
                Console.WriteLine("Backup service is started.\nPress <enter> to stop ...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                hostBackup.Close();
            }
        }

        public static void InitializeWindowsAuthentication(NetTcpBinding binding)
        {
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        }
    }
}
