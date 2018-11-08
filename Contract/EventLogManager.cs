using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public class EventLogManager
    {
        public EventLogManager()
        {
            if (!EventLog.SourceExists("ProjectEvents"))
            {
                EventLog.CreateEventSource("ProjectEvents", "CertificateLog");
            }
        }
    }
}
