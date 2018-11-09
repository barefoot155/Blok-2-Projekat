using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Contract
{
    [ServiceContract(CallbackContract = typeof(IDisconnectCallback))]
    public interface IWCFContracts
    {
        [OperationContract(IsOneWay = true)]
        void TestCommunication();
        [OperationContract(IsOneWay = true)]
        void SendMessage(string msg);
        [OperationContract]
        void PingServer(DateTime dt);
    }

    public interface IDisconnectCallback
    {
        [OperationContract]
        void DisconnectClient(string msg);
    }
}
