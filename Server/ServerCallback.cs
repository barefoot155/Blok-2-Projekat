using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;

namespace Server
{
    public class ServerCallback : ICertificateCallback
    {
        public void NotifyClients(string msg, string serverName)
        {
            Program.CloseServerConnection(serverName);
        }
    }
}
