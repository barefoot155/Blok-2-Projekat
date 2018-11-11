using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.Security.Principal;
using System.Diagnostics;

namespace Client
{

    public class DisconnectCallback : IDisconnectCallback
    {
        public void DisconnectClient(string msg)
        {
            Console.WriteLine("Disconnect CALLBACK");
            if (msg.ToLower().Equals("close"))
            { 
                Program.myChannel.Close();
                if (Program.myChannel.State == System.ServiceModel.CommunicationState.Closed)
                {
                    Console.WriteLine("Client closed connection with server.");
                }
            }
        }
    }
}
