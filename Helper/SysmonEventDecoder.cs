// <copyright file="SysmonEventDecoder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a contract for metadata information.
    /// </summary>
    /// <remarks>This interface serves as a marker for classes that provide metadata. Implementing this
    /// interface indicates that the SysmonEvent class contains metadata-related information.</remarks>
    public interface IMetadata { }

    /// <summary>
    /// Represents the base class for a Sysmon event, providing common properties for event data.
    /// </summary>
    /// <remarks>This class serves as a foundation for specific Sysmon event types, encapsulating shared
    /// properties such as timestamp, event code, event name, and source. Derived classes should implement the  <see
    /// cref="MetadataObject"/> property to provide event-specific metadata.</remarks>
    public abstract class SysmonEventBase
    {
        /// <summary>
        /// Gets or sets the timestamp of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(Timestamp))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the event code associated with the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(EventCode))]
        public int EventCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(EventName))]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the source of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(Source))]
        public string Source { get; set; }

        [JsonIgnore]
        public abstract IMetadata MetadataObject { get; }
    }

    /// <summary>
    /// Represents metadata associated with the creation of a process.
    /// </summary>
    /// <remarks>This class provides detailed information about a process, including its command line, 
    /// associated company, current directory, file version, and more. It also includes metadata  about the parent
    /// process and user context. This information can be useful for auditing,  monitoring, or analyzing process
    /// creation events.</remarks>
    public sealed class ProcessCreationMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the command line used to start the process.
        /// </summary>
        [JsonProperty(nameof(CommandLine))]
        public string CommandLine { get; set; }

        /// <summary>
        /// Gets or sets the company associated with the process image.
        /// </summary>
        [JsonProperty(nameof(Company))]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the current directory of the process.
        /// </summary>
        [JsonProperty(nameof(CurrentDirectory))]
        public string CurrentDirectory { get; set; }

        /// <summary>
        /// Gets or sets the file version of the process image.
        /// </summary>
        [JsonProperty(nameof(FileVersion))]
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the hash values of the process image.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the image name.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the integrity level of the process.
        /// </summary>
        [JsonProperty(nameof(IntegrityLevel))]
        public string IntegrityLevel { get; set; }

        /// <summary>
        /// Gets or sets the logon ID associated with the process.
        /// </summary>
        [JsonProperty(nameof(LogonId))]
        public string LogonId { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the process image.
        /// </summary>
        [JsonProperty(nameof(OriginalFileName))]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the command line of the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentCommandLine))]
        public string ParentCommandLine { get; set; }

        /// <summary>
        /// Gets or sets the parent image name associated with the network connection.
        /// </summary>
        [JsonProperty(nameof(ParentImage))]
        public string ParentImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentProcessId))]
        public long ParentProcessId { get; set; }

        /// <summary>
        /// Gets or sets the process ID.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the product name associated with the process image.
        /// </summary>
        [JsonProperty(nameof(Product))]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata related to process access events, including information about the source and target
    /// processes and users.
    /// </summary>
    /// <remarks>This class provides properties to access details about the source and target processes
    /// involved in an access event, such as their image names, process IDs, and associated users.</remarks>
    public sealed class ProcessAccessMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the image name of the source process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(SourceImage))]
        public string SourceImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the source process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(SourceProcessId))]
        public long SourceProcessId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the source process.
        /// </summary>
        [JsonProperty(nameof(SourceUser))]
        public string SourceUser { get; set; }

        /// <summary>
        /// Gets or sets the image name of the target process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(TargetImage))]
        public string TargetImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the target process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(TargetProcessId))]
        public long TargetProcessId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the target process.
        /// </summary>
        [JsonProperty(nameof(TargetUser))]
        public string TargetUser { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a terminated process.
    /// </summary>
    /// <remarks>This class provides information about a process that has been terminated, including its ID,
    /// the image name, and the user who initiated it.</remarks>
    public sealed class ProcessTerminatedMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID associated with the DNS query event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the DNS query event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata for a network connection, providing details about the source and destination endpoints, the
    /// protocol used, and the process initiating the connection.
    /// </summary>
    /// <remarks>This class is used to encapsulate information about a network connection, including both
    /// source and destination details such as IP addresses, hostnames, and ports. It also includes metadata about the
    /// process that initiated the connection, such as the process ID and GUID, as well as user and image information.
    /// This metadata can be useful for logging, monitoring, or analyzing network activity.</remarks>
    public sealed class NetworkConnectionMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the destination host name for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationHostname))]
        public string DestinationHostname { get; set; }

        /// <summary>
        /// Gets or sets the destination IP address for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationIp))]
        public string DestinationIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination IP is IPv6.
        /// </summary>
        [JsonProperty(nameof(DestinationIsIpv6))]
        public bool DestinationIsIpv6 { get; set; }

        /// <summary>
        /// Gets or sets the destination port for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationPort))]
        public long DestinationPort { get; set; }

        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection was initiated.
        /// </summary>
        [JsonProperty(nameof(Initiated))]
        public bool Initiated { get; set; }

        /// <summary>
        /// Gets or sets the process GUID associated with the network connection.
        /// </summary>
        [JsonProperty(nameof(ProcessGuid))]
        public string ProcessGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the protocol used for the network connection.
        /// </summary>
        [JsonProperty(nameof(Protocol))]
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the source host name for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourceHostname))]
        public string SourceHostname { get; set; }

        /// <summary>
        /// Gets or sets the source IP address for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourceIp))]
        public string SourceIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source IP is IPv6.
        /// </summary>
        [JsonProperty(nameof(SourceIsIpv6))]
        public bool SourceIsIpv6 { get; set; }

        /// <summary>
        /// Gets or sets the source port for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourcePort))]
        public long SourcePort { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the current operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent image.
        /// </summary>
        [JsonProperty(nameof(ParentImage))]
        public string ParentImage { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a DNS query, including details about the process and user that initiated the
    /// query.
    /// </summary>
    /// <remarks>This class provides properties to access information such as the image name of the process,
    /// the process ID, the DNS query name, and the user who initiated the query.</remarks>
    public sealed class DnsQueryMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the URL of the image associated with this entity.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the DNS query name.
        /// </summary>
        [JsonProperty(nameof(QueryName))]
        public string QueryName { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the current operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

    }

    /// <summary>
    /// Represents metadata associated with a driver load, including hash and signature information.
    /// </summary>
    /// <remarks>This class provides properties to access the hash values and signature details of a driver,
    /// which can be used for validation and verification purposes.</remarks>
    public sealed class DriverLoadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the hash values associated with the object.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the digital signature associated with the object.
        /// </summary>
        [JsonProperty(nameof(Signature))]
        public string Signature { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with an image load event in a process.
    /// </summary>
    /// <remarks>This class provides properties to access various details about the image load, such as the
    /// company name, file version, and process ID. It is typically used in scenarios where detailed information about
    /// loaded images is required for logging, auditing, or analysis purposes.</remarks>
    public sealed class ImageLoadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the name of the company associated with the entity.
        /// </summary>
        [JsonProperty(nameof(Company))]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        [JsonProperty(nameof(FileVersion))]
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the hash values associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the image.
        /// </summary>
        [JsonProperty(nameof(OriginalFileName))]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the product name associated with the image.
        /// </summary>
        [JsonProperty(nameof(Product))]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a file event, including process information and file details.
    /// </summary>
    /// <remarks>This class provides properties to access various details about a file event, such as the
    /// process ID, the image name, the target filename, the creation time in UTC, and the user associated with the
    /// event.</remarks>
    public sealed class FileEventMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID associated with the file event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the file event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the target filename involved in the file event.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the file event in UTC.
        /// </summary>
        [JsonProperty(nameof(CreationUtcTime))]
        public string CreationUtcTime { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the file event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the raw access read permissions as a string.
        /// </summary>
        [JsonProperty(nameof(RawAccessRead), NullValueHandling = NullValueHandling.Ignore)]
        public string RawAccessRead { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a file deletion operation.
    /// </summary>
    /// <remarks>This class provides information about the file deletion process, including the image name,
    /// process ID, target filename, and user responsible for the operation. It is intended to be used in scenarios
    /// where tracking or auditing of file deletions is required.</remarks>
    public sealed class FileDeleteMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target filename that was deleted.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a registry addition event.
    /// </summary>
    /// <remarks>This class provides information about a registry addition, including the process ID,  the
    /// image name, the target object, and the user responsible for the action.</remarks>
    public sealed class RegistryAddMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry addition operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry addition.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry addition.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry addition operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a registry delete operation.
    /// </summary>
    /// <remarks>This class provides details about a registry delete operation, including the image involved,
    /// the process ID, and the target object of the operation. It is used to encapsulate information relevant to the
    /// deletion event.</remarks>
    public sealed class RegistryDeleteMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a registry set operation.
    /// </summary>
    /// <remarks>This class provides details about a registry set operation, including the user who performed
    /// the operation, the target object affected, and additional descriptive information.</remarks>
    public sealed class RegistrySetMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry set operation.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry set operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry set operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry set operation.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry set operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata associated with a registry event.
    /// </summary>
    /// <remarks>This class provides details about a registry event, including information about the process
    /// and user involved.</remarks>
    public sealed class RegistryEventMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry event.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry event.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

    /// <summary>
    /// Represents metadata for creating a stream hash, including details such as creation time, hash value, and
    /// associated user.
    /// </summary>
    /// <remarks>This class is used to encapsulate metadata information required for the creation of a stream
    /// hash. It includes properties for the creation time in UTC, the hash value, the image associated with the hash,
    /// the process ID, the target filename, and the user who initiated the creation.</remarks>
    public sealed class CreateStreamHashMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the creation time of the stream hash in UTC.
        /// </summary>
        [JsonProperty(nameof(CreationUtcTime))]
        public string CreationUtcTime { get; set; }

        /// <summary>
        /// Gets or sets the hash value of the stream.
        /// </summary>
        [JsonProperty(nameof(Hash))]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the image associated with the stream hash.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the stream hash creation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target filename for which the stream hash was created.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the stream hash creation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }

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

        // represent for file_event
        private const ushort FILECREATE = 11;
        private const ushort RAWACCESSREAD = 9;
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
                case FILECREATE:
                case FILEEXEDETECTED:
                case RAWACCESSREAD:
                    return SysEventCode.FileEvent;
                case FILEDELETE:
                case FILEDELETEDETECTED:
                    return SysEventCode.FileDelete;
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

            eventObj.Add("EventName", value: SysmonEventCodeExtensions.ToFriendlyString(eventCode));

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
                case SysEventCode.FileEvent:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileEventMetadata>(origin);
                    break;
                case SysEventCode.FileDelete:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<FileDeleteMetadata>(origin);
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
                case SysEventCode.CreateStreamHash:
                    metadataObj = SysmonEventDecoder.DecodeSysmonMetadata<CreateStreamHashMetadata>(origin);
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
                throw new EventSourceException("there is no EventType in the log");
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
