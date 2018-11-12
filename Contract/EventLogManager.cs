using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public static class EventLogManager
    {
        private static EventLog generateCertLog = new EventLog();
        private static EventLog generateServerLog = new EventLog();

        public static void InitializeCMSEventLog()
        {
            string source = "CerificateEvents";
            string logName = "CertificateLog";

            try
            {
                if (!EventLog.SourceExists(source)) //ako baca exception potrebno je u regedit dodeli full control prava korisniku koji pokrece CertificateServiceManager
                {
                    EventLog.CreateEventSource(source, logName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[CMSEventLog] Exception: " + e.Message);
            }


            generateCertLog.Source = source;
            generateCertLog.Log = logName;
        }

        public static void InitializeServerEventLog()
        {
            string source = "ServerEvents";
            string logName = "ServerLog";

            try
            {
                if (!EventLog.SourceExists(source)) //ako baca exception potrebno je u regedit dodeli full control prava korisniku koji pokrece CertificateServiceManager
                {
                    EventLog.CreateEventSource(source, logName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[ServerEventLog] Exception: " + e.Message);
            }
            generateServerLog.Source = source;
            generateServerLog.Log = logName;
        }

        public static void WriteEntryCMS(string message, EventLogEntryType evntType, int eventID)
        {
            generateCertLog.WriteEntry(message, evntType, eventID);
        }

        public static void WriteEntryServer(string message, EventLogEntryType evntType, int eventID)
        {
            generateServerLog.WriteEntry(message, evntType, eventID);
        }
    }
}
