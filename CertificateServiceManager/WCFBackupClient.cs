using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CertificateServiceManager
{
    public class WCFBackupClient : ChannelFactory<IBackupData>, IBackupData, IDisposable
    {
        IBackupData proxy;
        public WCFBackupClient(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            proxy = this.CreateChannel();
        }

        public void ReplicateCertificate(string cert)
        {
            proxy.ReplicateCertificate(cert);
        }

        public void ReplicateRevokedCert(string cert)
        {
            proxy.ReplicateRevokedCert(cert);
        }
    }
}
