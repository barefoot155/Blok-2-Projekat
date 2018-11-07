using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

namespace CertificateServiceManager
{
    public class CertificateManager : ICertificateManager
    {
        public X509Certificate2 GenerateCertificate(string root)
        {
            Process p = new Process();
            string path = (AppDomain.CurrentDomain.BaseDirectory + @"\makecert.exe");
            string userName = Thread.CurrentPrincipal.Identity.Name.Split('\\')[1];

            //get groups from windowsIdentity.Groups
            string groups = GetUserGroups((Thread.CurrentPrincipal.Identity as WindowsIdentity));

            string arguments = string.Format("-sv {0}.pvk -iv {1}.pvk -n \"CN = {0},OU={2}\" -pe -ic {1}.cer {0}.cer -sr localmachine -ss My -sky exchange", userName, root, groups);
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            p.Dispose();

            //create .pfx file
            Process p2 = new Process();
            path = (AppDomain.CurrentDomain.BaseDirectory + @"\pvk2pfx.exe");
            arguments = string.Format("/pvk {0}.pvk /pi 123 /spc {0}.cer /pfx {0}.pfx", userName);
            info = new ProcessStartInfo(path, arguments);
            p2.StartInfo = info;
            p2.Start();
            p2.WaitForExit();

            p2.Dispose();
            return null;
        }

        private string GetUserGroups(WindowsIdentity windowsIdentity)
        {
            string groups = "";
            foreach (IdentityReference group in windowsIdentity.Groups)
            {
                SecurityIdentifier sid =
                (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount)).ToString();
                if (name.Contains('\\'))
                    name = name.Split('\\')[1]; 
                if (name == "RegionWest" || name == "RegionEast" || name == "RegionNorth" || name == "RegionSouth")
                {
                    if (groups != "")
                        groups += "_" + name;
                    else
                        groups = name;
                    
                }

            }

            return groups;
        }

        public void RevokeCertificate()
        {
            throw new NotImplementedException();
        }
    }
}
