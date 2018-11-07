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

namespace Server
{
    public class ServiceImplementation : IWCFContracts
    {
        private EventLog connectionEvent = new EventLog();
        private string message = string.Empty;

        public ServiceImplementation()
        {
            if (!EventLog.SourceExists("ConnectionEvents"))
            {
                EventLog.CreateEventSource("ConnectionEvents", "ConnectionLog");
            }

            connectionEvent.Source = "ConnectionEvents";
            connectionEvent.Log = "ConnectionLog";

            message = String.Format("Client {0} established connection with server.", ServiceSecurityContext.Current.PrimaryIdentity.Name);
            EventLogEntryType evntType = EventLogEntryType.SuccessAudit;

            connectionEvent.WriteEntry(message, evntType);
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
            if (!isClientAuthorized())
                throw new SecurityException("Access denied");

            X509Certificate2 clientCert = getClientCertificate();
            int commaIndex = clientCert.SubjectName.Name.IndexOf(',');
            string commonName = clientCert.SubjectName.Name.Remove(commaIndex); //delete everything after comma -> leave only CN="username"
            Logger.LogData(dt, commonName);
        }

        public void TestCommunication()
        {
            Console.WriteLine("Communication is established...");
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
