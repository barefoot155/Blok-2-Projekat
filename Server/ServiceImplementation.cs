using Contract;
using System;
using System.Collections.Generic;
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
        public void SendMessage(string msg)
        {
            if (!isClientAuthorized())
                throw new SecurityException("Access denied");

            Console.WriteLine("Client sent msg: {0}", msg);

            //log to file
        }

        public void TestCommunication()
        {            
            Console.WriteLine("Communication is established...");
        }

        private bool isClientAuthorized()
        {
            try
            {
                X509Certificate2 clientCert = ((X509CertificateClaimSet)
                    OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate; //gets client certificate from connection

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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
