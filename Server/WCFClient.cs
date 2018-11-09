using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class WCFClient : DuplexChannelFactory<ICertificateManager>, ICertificateManager, IDisposable
    {
        ICertificateManager proxy;
        public WCFClient(object callbackInstance, NetTcpBinding binding, EndpointAddress address)
            : base(callbackInstance, binding, address)
        {

            proxy = this.CreateChannel();
        }

        public X509Certificate2 GenerateCertificate(string root)
        {
           return proxy.GenerateCertificate(root);
        }

        public void RevokeCertificate(X509Certificate2 certificate)
        {
            proxy.RevokeCertificate(certificate);
        }

        public void SendMessage(string msg, byte[] sign)
        {
            throw new NotImplementedException();
        }

        public void TestCommunication()
        {
            throw new NotImplementedException();
        }
    }
}
