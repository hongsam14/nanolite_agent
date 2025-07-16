// <copyright file="Sysmon.cs" company="Hongsam14">
// Copyright (c) Hongsam14. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    public interface IMetadata { }
    public abstract class SysmonEventBase
    {
        [JsonProperty("Timestamp")] public DateTime Timestamp { get; set; }

        [JsonProperty("EventCode")] public int EventCode { get; set; }

        [JsonProperty("EventName")] public string EventName { get; set; }

        [JsonProperty("Source")] public string Source { get; set; }

        [JsonIgnore] public abstract IMetadata MetadataObject { get; }
    }

    public sealed class ProcessCreationMetadata : IMetadata
    {
        [JsonProperty("CommandLine")] public string CommandLine { get; set; }

        [JsonProperty("Company")] public string Company { get; set; }

        [JsonProperty("CurrentDirectory")] public string CurrentDirectory { get; set; }

        [JsonProperty("FileVersion")] public string FileVersion { get; set; }

        [JsonProperty("Hashes")] public string Hashes { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("IntegrityLevel")] public string IntegrityLevel { get; set; }

        [JsonProperty("LogonId")] public string LogonId { get; set; }

        [JsonProperty("OriginalFileName")] public string OriginalFileName { get; set; }

        [JsonProperty("ParentCommandLine")] public string ParentCommandLine { get; set; }

        [JsonProperty("ParentImage")] public string ParentImage { get; set; }

        [JsonProperty("ParentProcessId")] public long ParentProcessId { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Product")] public string Product { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class ProcessAccessMetadata : IMetadata
    {
        [JsonProperty("SourceImage")] public string SourceImage { get; set; }

        [JsonProperty("SourceProcessId")] public long SourceProcessId { get; set; }

        [JsonProperty("SourceUser")] public string SourceUser { get; set; }

        [JsonProperty("TargetImage")] public string TargetImage { get; set; }

        [JsonProperty("TargetProcessId")] public long TargetProcessId { get; set; }

        [JsonProperty("TargetUser")] public string TargetUser { get; set; }
    }

    public sealed class ProcessTerminatedMetadata : IMetadata
    {
        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("User")] public string User { get; set; }

    }

    public sealed class NetworkConnectionMetadata : IMetadata
    {
        [JsonProperty("DestinationHostname")] public string DestinationHostname { get; set; }

        [JsonProperty("DestinationIp")] public string DestinationIp { get; set; }

        [JsonProperty("DestinationIsIpv6")] public bool DestinationIsIpv6 { get; set; }

        [JsonProperty("DestinationPort")] public long DestinationPort { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("Initiated")] public bool Initiated { get; set; }

        [JsonProperty("ProcessGuid")] public string ProcessGuid { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Protocol")] public string Protocol { get; set; }

        [JsonProperty("SourceHostname")] public string SourceHostname { get; set; }

        [JsonProperty("SourceIp")] public string SourceIp { get; set; }

        [JsonProperty("SourceIsIpv6")] public bool SourceIsIpv6 { get; set; }

        [JsonProperty("SourcePort")] public long SourcePort { get; set; }

        [JsonProperty("User")] public string User { get; set; }

        [JsonProperty("ParentImage")] public string ParentImage { get; set; }
    }

    public sealed class DnsQueryMetadata : IMetadata
    {
        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("QueryName")] public string QueryName { get; set; }

        [JsonProperty("User")] public string User { get; set; }

    }

    public sealed class DriverLoadMetadata : IMetadata
    {
        [JsonProperty("Hashes")] public string Hashes { get; set; }

        [JsonProperty("Signature")] public string Signature { get; set; }
    }

    public sealed class ImageLoadMetadata : IMetadata
    {
        [JsonProperty("Company")] public string Company { get; set; }

        [JsonProperty("FileVersion")] public string FileVersion { get; set; }

        [JsonProperty("Hashes")] public string Hashes { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("OriginalFileName")] public string OriginalFileName { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Product")] public string Product { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class FileEventMetadata : IMetadata
    {
        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("TargetFilename")] public string TargetFilename { get; set; }

        [JsonProperty("CreationUtcTime")] public string CreationUtcTime { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class FileDeleteMetadata : IMetadata
    {
        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("TargetFilename")] public string TargetFilename { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class RegistryAddMetadata : IMetadata
    {
        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("TargetObject")] public string TargetObject { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class RegistryDeleteMetadata : IMetadata
    {
        [JsonProperty("Details")] public string Details { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("TargetObject")] public string TargetObject { get; set; }
    }

    public sealed class RegistrySetMetadata : IMetadata
    {
        [JsonProperty("Details")] public string Details { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("TargetObject")] public string TargetObject { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class RegistryEventMetadata : IMetadata
    {
        [JsonProperty("Details")] public string Details { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("TargetObject")] public string TargetObject { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }

    public sealed class CreateStreamHashMetadata : IMetadata
    {
        [JsonProperty("CreationUtcTime")] public string CreationUtcTime { get; set; }

        [JsonProperty("Hash")] public string Hash { get; set; }

        [JsonProperty("Image")] public string Image { get; set; }

        [JsonProperty("ProcessId")] public long ProcessId { get; set; }

        [JsonProperty("TargetFilename")] public string TargetFilename { get; set; }

        [JsonProperty("User")] public string User { get; set; }
    }
    public static class SysmonEventDecoder
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

        public static JObject DecodeSysmonEvent(TraceEvent origin)
        {
            SysEventCode code = SysmonEventDecoder.GetEventCodeFromData(origin);
            return SysmonEventDecoder.EventFromCode(code, origin);
        }

        private static JObject EventFromCode(SysEventCode eventCode, TraceEvent origin)
        {
            JObject metadataObj, eventObj;

            eventObj = new JObject();

            // add common header
            eventObj.Add("EventID", (int)eventCode);
            eventObj.Add("EventName", eventCode.ToString().ToLower());
            eventObj.Add("Source", "sysmon");
            eventObj.Add("TimeStamp", origin.TimeStamp);
            
            switch (eventCode)
            {
                case SysEventCode.PROCESS_CREATION:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessCreationMetadata>(origin);
                    break;
                case SysEventCode.PROCESS_TERMINATED:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessTerminatedMetadata>(origin);
                    break;
                case SysEventCode.PROCESS_ACCESS:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ProcessAccessMetadata>(origin);
                    break;
                case SysEventCode.FILE_EVENT:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileEventMetadata>(origin);
                    break;
                case SysEventCode.FILE_DELETE:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileDeleteMetadata>(origin);
                    break;
                case SysEventCode.DRIVER_LOAD:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<DriverLoadMetadata>(origin);
                    break;
                case SysEventCode.IMAGE_LOAD:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<ImageLoadMetadata>(origin);
                    break;
                case SysEventCode.REGISTRY_SET:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistrySetMetadata>(origin);
                    break;
                case SysEventCode.REGISTRY_ADD:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistryAddMetadata>(origin);
                    break;
                case SysEventCode.REGISTRY_DELETE:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<RegistryDeleteMetadata>(origin);
                    break;
                case SysEventCode.CREATE_STREAM_HASH:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<CreateStreamHashMetadata>(origin);
                    break;
                case SysEventCode.DNS_QUERY:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<DnsQueryMetadata>(origin);
                    break;
                case SysEventCode.NETWORK_CONNECTION:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<NetworkConnectionMetadata>(origin);
                    break;
                default:
                    // just return null
                    return null;
            }

            eventObj.Add("Metadata", metadataObj);
            return eventObj;
        }

        private static JObject DecodeSysmonMetadata<T>(TraceEvent origin) where T : IMetadata
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

        private static SysEventCode GetEventCodeFromData(TraceEvent eventData)
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
                    return SysmonEventDecoder.GetRegistryTypeFromData(eventData);
                case REGISTRYSETVALUE:
                    return SysEventCode.REGISTRY_SET;
                default:
                    return SysEventCode.UNKNOWN;
            }
        }

        private static SysEventCode GetRegistryTypeFromData(TraceEvent regEventtData)
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

    public class Sysmon : BaseTracepoint
    {
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

            log = SysmonEventDecoder.DecodeSysmonEvent(data);
            if (log == null)
            {
                throw new ArgumentException("trace event data is not acceptable");
            }

            return log;
        }
    }
}
