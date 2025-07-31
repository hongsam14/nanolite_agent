// <copyright file="SysmonEventDecoder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Nanolite_agent.EventModel;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides functionality to decode Sysmon events from trace event data.
    /// </summary>
    /// <remarks>This static class offers methods to interpret and convert raw trace event data into
    /// structured JSON objects representing various Sysmon event types. It supports decoding events such as process
    /// creation, file operations, network connections, and registry modifications.</remarks>
    public static class SysmonEventDecoder
    {
        // represent for process_creation
        private const ushort PROCESSCREATE = 1;

        // represent for process_terminated
        private const ushort PROCESSTERMINATED = 5;

        // represent for process_access
        private const ushort PROCESSACCESS = 10;

        // represent for create_remote_thread
        private const ushort CREATEREMOTETHREAD = 8;

        // represent for process_tampering
        private const ushort PROCESSTAMPERING = 25;

        private const ushort RAWACCESSREAD = 9;

        // represent for file_create
        private const ushort FILECREATE = 11;
        private const ushort FILEEXEDETECTED = 29;

        // represent for file modified
        private const ushort FILECREATIONTIMECHANGED = 2;

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

        /// <summary>
        /// Decodes a Sysmon event from the provided trace event data.
        /// </summary>
        /// <param name="origin">The trace event data to decode. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded Sysmon event.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        public static JObject DecodeSysmonEvent(TraceEvent origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            SysEventCode code = SysmonEventDecoder.GetEventCodeFromData(origin);
            return SysmonEventDecoder.EventFromCode(code, origin);
        }

        /// <summary>
        /// Determines the system event code based on the provided trace event data.
        /// </summary>
        /// <param name="eventData">The trace event data from which to derive the system event code. Cannot be null.</param>
        /// <returns>A <see cref="SysEventCode"/> value representing the type of system event detected in the trace event data.
        /// Returns <see cref="SysEventCode.Unknown"/> if the event ID does not match any known event types.</returns>
        public static SysEventCode GetEventCodeFromData(TraceEvent eventData)
        {
            switch ((ushort)eventData.ID)
            {
                case PROCESSCREATE:
                    return SysEventCode.ProcessCreation;
                case PROCESSTERMINATED:
                    return SysEventCode.ProcessTerminated;
                case PROCESSACCESS:
                    return SysEventCode.ProcessAccess;
                case PROCESSTAMPERING:
                    return SysEventCode.ProcessTampering;
                case CREATEREMOTETHREAD:
                    return SysEventCode.CreateRemoteThread;
                case FILECREATE:
                case FILEEXEDETECTED:
                    return SysEventCode.FileCreate;
                case FILEDELETE:
                case FILEDELETEDETECTED:
                    return SysEventCode.FileDelete;
                case RAWACCESSREAD:
                    return SysEventCode.RawAccessReadDetected;
                case FILECREATIONTIMECHANGED:
                    return SysEventCode.FileModified;
                case FILECREATESTREAMHASH:
                    return SysEventCode.CreateStreamHash;
                case NETWORKCONNECT:
                    return SysEventCode.NetworkConnection;
                case DNSQUERY:
                    return SysEventCode.DnsQuery;
                case DRIVERLOADED:
                    return SysEventCode.DriverLoad;
                case IMAGELOADED:
                    return SysEventCode.ImageLoad;
                case REGISTRYCREATEDELETE:
                    return SysmonEventDecoder.GetRegistryTypeFromData(eventData);
                case REGISTRYSETVALUE:
                    return SysEventCode.RegistrySet;
                case REGISTRYRENAME:
                    return SysEventCode.RegistryRename;
                default:
                    return SysEventCode.Unknown;
            }
        }

        private static JObject EventFromCode(SysEventCode eventCode, TraceEvent origin)
        {
            JObject metadataObj, eventObj;

            eventObj = new JObject();

            // add common header
            eventObj.Add("EventID", (int)eventCode);

            eventObj.Add("EventName", value: Helper.SysEventCodeExtension.ToFriendlyString(eventCode));

            eventObj.Add("Source", "sysmon");

            eventObj.Add("TimeStamp", origin.TimeStamp);

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessCreationMetadata>(origin);
                    break;
                case SysEventCode.ProcessTerminated:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessTerminatedMetadata>(origin);
                    break;
                case SysEventCode.ProcessAccess:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessAccessMetadata>(origin);
                    break;
                case SysEventCode.ProcessTampering:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessTamperingMetadata>(origin);
                    break;
                case SysEventCode.CreateRemoteThread:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<CreateRemoteThreadMetadata>(origin);
                    break;
                case SysEventCode.FileCreate:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileCreateMetadata>(origin);
                    break;
                case SysEventCode.FileDelete:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileDeleteMetadata>(origin);
                    break;
                case SysEventCode.FileModified:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileModifiedMetadata>(origin);
                    break;
                case SysEventCode.RawAccessReadDetected:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RawAccessReadMetadata>(origin);
                    break;
                case SysEventCode.CreateStreamHash:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<CreateStreamHashMetadata>(origin);
                    break;
                case SysEventCode.DriverLoad:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<DriverLoadMetadata>(origin);
                    break;
                case SysEventCode.ImageLoad:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ImageLoadMetadata>(origin);
                    break;
                case SysEventCode.RegistrySet:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistrySetMetadata>(origin);
                    break;
                case SysEventCode.RegistryAdd:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistryAddMetadata>(origin);
                    break;
                case SysEventCode.RegistryDelete:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistryDeleteMetadata>(origin);
                    break;
                case SysEventCode.RegistryRename:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistryRenameMetadata>(origin);
                    break;
                case SysEventCode.DnsQuery:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<DnsQueryMetadata>(origin);
                    break;
                case SysEventCode.NetworkConnection:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<NetworkConnectionMetadata>(origin);
                    break;
                default:
                    // just return null
                    return null;
            }

            eventObj.Add("Metadata", metadataObj);
            return eventObj;
        }

        private static JObject DecodeSysmonMetadata<T>(TraceEvent origin)
            where T : IMetadata
        {
            JObject originObj;
            T convertedObj;

            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            // make new Json Object from TraceEvent
            originObj = new JObject();

            // copy all payloads to JObject
            for (int i = 0; i < origin.PayloadNames.Length; i++)
            {
                originObj.Add(origin.PayloadNames[i], JToken.FromObject(origin.PayloadValue(i)));
            }

            // convert originObj to Metadata Object to filter unrelated value
            convertedObj = originObj.ToObject<T>();

            // convert T to JObject
            return JObject.FromObject(convertedObj);
        }

        private static SysEventCode GetRegistryTypeFromData(TraceEvent regEventtData)
        {
            object typeObj;

            // check eventData is not null
            if (regEventtData == null)
            {
                throw new ArgumentNullException(nameof(regEventtData));
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
                throw new SystemActivityException("EventType field is missing from the registry event data.");
            }

            switch (typeObj.ToString())
            {
                case "CreateKey":
                case "CreateValue":
                    return SysEventCode.RegistryAdd;
                case "DeleteKey":
                case "DeleteValue":
                    return SysEventCode.RegistryDelete;
                default:
                    return SysEventCode.Unknown;
            }
        }
    }
}
