using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Contract
{
    public class CMSClient : DuplexChannelFactory<ICertificateManager>, ICertificateManager, IDisposable
    {
        ICertificateManager proxy;
        public CMSClient(object callbackInstance, NetTcpBinding binding, EndpointAddress address)
            : base(callbackInstance, binding, address)
        {
            proxy = this.CreateChannel();
        }

        public void GenerateCertificate(string root)
        {
           proxy.GenerateCertificate(root);
        }

        public void GenerateCertificateWithoutPVK(string root)
        {
            proxy.GenerateCertificateWithoutPVK(root);
        }

        public void RegisterClient()
        {
            proxy.RegisterClient();
        }

        public void RevokeCertificate(X509Certificate2 certificate)
        {
            proxy.RevokeCertificate(certificate);
        }
    }
}
