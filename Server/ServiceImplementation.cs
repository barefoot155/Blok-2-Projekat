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
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ServiceImplementation : IWCFContracts
    {
        private EventLog connectionEvent = new EventLog();
        private string message = string.Empty;
        private System.Timers.Timer disconnectTimer = new System.Timers.Timer();

        public ServiceImplementation()
        {
            message = String.Format("Client {0} established connection with server.", ServiceSecurityContext.Current.PrimaryIdentity.Name);
            EventLogEntryType evntType = EventLogEntryType.SuccessAudit;

            EventLogManager.WriteEntryServer(message, evntType);
        }
        /// <summary>
        /// Test metoda, ne koristi se u projektu
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            if (!isClientAuthorized())
                throw new SecurityException("Access denied");

            Console.WriteLine("Client sent msg: {0}", msg);
        }

        /// <summary>
        /// Called every 1-10s
        /// </summary>
        /// <param name="dt"></param>
        public void PingServer(DateTime dt)
        {
            if(disconnectTimer.Enabled)
            {
                disconnectTimer.Stop();
                
                disconnectTimer.Start();
            }
            if (!isClientAuthorized())
                throw new SecurityException("Access denied");

            X509Certificate2 clientCert = getClientCertificate();
            string commonName = Helper.ExtractCommonNameFromCertificate(clientCert);
            Logger.LogData(dt, commonName);

            disconnectTimer.Interval = 10000;
            disconnectTimer.Enabled = true;
            disconnectTimer.AutoReset = false;

            IDisconnectCallback currentClient = OperationContext.Current.GetCallbackChannel<IDisconnectCallback>();
            string clientIpAddress = (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;

            disconnectTimer.Elapsed += (sender, e) => DisconnectTimer_Elapsed(sender, e, currentClient, clientIpAddress);
        }

        private void DisconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e, IDisconnectCallback client, string clientIP)
        {
            message = String.Format("Client {0} disconnect from server.", clientIP);
            EventLogEntryType evntType = EventLogEntryType.SuccessAudit;
            EventLogManager.WriteEntryServer(message, evntType);

            //send disconnect
            client.DisconnectClient("close");
        }

        public void TestCommunication()
        {
           Console.WriteLine("Communication is established...");
            //add client to list
            IDisconnectCallback callback = OperationContext.Current.GetCallbackChannel<IDisconnectCallback>();
            if (!Program.myClients.Contains(callback))
                Program.myClients.Add(callback);


            // if (!EventLog.SourceExists("ProjectEvents"))
            // {
            //     EventLog.CreateEventSource("ProjectEvents", "ConnectionLog");
            // }

            // connectionEvent.Source = "ProjectEvents";
            // connectionEvent.Log = "ConnectionLog";

            //message = String.Format("Client {0} established connection with server.", ServiceSecurityContext.Current.PrimaryIdentity.Name);
            //EventLogEntryType evntType = EventLogEntryType.SuccessAudit;

            //connectionEvent.WriteEntry(message, evntType);
        }

        private bool isClientAuthorized()
        {
            try
            {
                X509Certificate2 clientCert = getClientCertificate(); 

                string subjectName = clientCert.SubjectName.Name;

                if (subjectName.Contains("RegionWest"))
                    return true;
                if (subjectName.Contains("RegionEast"))
                    return true;
                if (subjectName.Contains("RegionNorth"))
                    return true;
                if (subjectName.Contains("RegionSouth"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets client certificate from established connection
        /// </summary>
        /// <returns></returns>
        private X509Certificate2 getClientCertificate()
        {
            return ((X509CertificateClaimSet)
                    OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
        }
    }
}
