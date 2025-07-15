// <copyright file="Sysmon.cs" company="Hongsam14">
// Copyright (c) Hongsam14. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json.Linq;
    using System;

    public class Sysmon : BaseTracepoint
    {
        // represent for process_creation
        private const ushort PROCESSCREATE = 1;
        // represent for process_terminated
        private const ushort PROCESSTERMINATED = 5;
        // represent for process_access
        private const ushort PROCESSACCESS = 10;
        // represent for file_event
        private const ushort FILECREATE = 11;
        private const ushort FILEEXEDETECTED = 29;
        // represent for create_stream_hash
        private const ushort FILECREATESTREAMHASH = 15;
        // represent for file_delete
        private const ushort FILEDELETE = 23;
        private const ushort FILEDELETEDETECTED = 26;
        // represent for network_connect
        private const ushort NETWORKCONNECT = 3;
        // represent for dns_query
        private const ushort DNSQUERY = 22;
        // represent for driver load
        private const ushort DRIVERLOADED = 4;
        // represent for image load
        private const ushort IMAGELOADED = 7;
        // represent registry add / delete
        private const ushort REGISTRYCREATEDELETE = 12;
        // represent registry set
        private const ushort REGISTRYSETVALUE = 13;
        // represent registry rename
        private const ushort REGISTRYRENAME = 14;

        public Sysmon()
            : base("sysmon")
        {
        }

        public JObject GetSysmonLog(TraceEvent data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            SysEventCode eventCode = this.getEventCodeFromData(data);

            if ((int)eventCode < 0)
            {
                return null;
            }

            log = base.EventLog(eventCode, data);
            for (int i = 0; i < data.PayloadNames.Length; i++)
            {
                log.Add(data.PayloadNames[i], data.PayloadValue(i).ToString());
            }

            return log;
        }

        private SysEventCode getEventCodeFromData(TraceEvent eventData)
        {
            switch ((ushort)eventData.ID)
            {
                case PROCESSCREATE:
                    return SysEventCode.PROCESS_CREATION;
                case PROCESSTERMINATED:
                    return SysEventCode.PROCESS_TERMINATED;
                case PROCESSACCESS:
                    return SysEventCode.PROCESS_ACCESS;
                case FILECREATE:
                case FILEEXEDETECTED:
                    return SysEventCode.FILE_EVENT;
                case FILEDELETE:
                case FILEDELETEDETECTED:
                    return SysEventCode.FILE_DELETE;
                case FILECREATESTREAMHASH:
                    return SysEventCode.CREATE_STREAM_HASH;
                case NETWORKCONNECT:
                    return SysEventCode.NETWORK_CONNECTION;
                case DNSQUERY:
                    return SysEventCode.DNS_QUERY;
                case DRIVERLOADED:
                    return SysEventCode.DRIVER_LOAD;
                case IMAGELOADED:
                    return SysEventCode.IMAGE_LOAD;
                case REGISTRYCREATEDELETE:
                    return this.getRegistryTypeFromData(eventData);
                case REGISTRYSETVALUE:
                    return SysEventCode.REGISTRY_SET;
                case REGISTRYRENAME:
                    return SysEventCode.REGISTRY_RENAME;
                default:
                    return SysEventCode.UNKNOWN;
            }
        }

        private SysEventCode getRegistryTypeFromData(TraceEvent regEventtData)
        {
            object typeObj;

            // check eventData is not null
            if (regEventtData == null)
            {
                throw new ArgumentNullException("regEventData");
            }

            // check if regEventData is not register event code
            if ((ushort)regEventtData.ID != REGISTRYCREATEDELETE)
            {
                throw new ArgumentException("regEventData should have eventcode 12");
            }

            // get EventType from payload
            typeObj = regEventtData.PayloadByName("EventType");
            if (typeObj == null)
            {
                throw new EventSourceException("there is no EventType in the log");
            }

            switch (typeObj.ToString())
            {
                case "CreateKey":
                case "CreateValue":
                    return SysEventCode.REGISTRY_ADD;
                case "DeleteKey":
                case "DeleteValue":
                    return SysEventCode.REGISTRY_DELETE;
                default:
                    return SysEventCode.UNKNOWN;
            }
        }
    }
}
