// <copyright file="SysEventCode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    /// <summary>
    /// Represents the event code for portion of Sysmon Events.
    /// </summary>
    public enum SysEventCode
    {
        /// <summary>
        /// Represents the event code for an unknown event.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Represents the event code for process creation.
        /// </summary>
        ProcessCreation = 0,

        /// <summary>
        /// Represents the event code for process access.
        /// </summary>
        ProcessAccess,

        /// <summary>
        /// Represents the event code for process termination.
        /// </summary>
        ProcessTerminated,

        /// <summary>
        /// Represents the event code for a network connection.
        /// </summary>
        NetworkConnection,

        /// <summary>
        /// Represents the event code for a DNS query.
        /// </summary>
        DnsQuery,

        /// <summary>
        /// Represents the event code for a driver load.
        /// </summary>
        DriverLoad,

        /// <summary>
        /// Represents the event code for an image load.
        /// </summary>
        ImageLoad,

        /// <summary>
        /// Represents the event code for a file event.
        /// </summary>
        FileEvent,

        /// <summary>
        /// Represents the event code for creating a stream hash.
        /// </summary>
        CreateStreamHash,

        /// <summary>
        /// Represents the event code for file deletion.
        /// </summary>
        FileDelete,

        /// <summary>
        /// Represents the event code for adding a registry entry.
        /// </summary>
        RegistryAdd,

        /// <summary>
        /// Represents the event code for deleting a registry entry.
        /// </summary>
        RegistryDelete,

        /// <summary>
        /// Represents the event code for setting a registry value.
        /// </summary>
        RegistrySet,
    }

    /// <summary>
    /// Provides extension methods for the <see cref="SysEventCode"/> enumeration.
    /// </summary>
    /// <remarks>This class includes methods to convert <see cref="SysEventCode"/> values to more readable
    /// string representations.</remarks>
    public static class SysmonEventCodeExtensions
    {
        /// <summary>
        /// Converts the <see cref="SysEventCode"/> value to a user-friendly string representation.
        /// </summary>
        /// <param name="code">The <see cref="SysEventCode"/> to convert.</param>
        /// <returns>A user-friendly string representation of the event code.</returns>
        public static string ToFriendlyString(this SysEventCode code)
        {
            switch (code)
            {
                case SysEventCode.ProcessCreation:
                    return "process_creation";
                case SysEventCode.ProcessAccess:
                    return "process_access";
                case SysEventCode.ProcessTerminated:
                    return "process_terminated";
                case SysEventCode.NetworkConnection:
                    return "network_connection";
                case SysEventCode.DnsQuery:
                    return "dns_query";
                case SysEventCode.DriverLoad:
                    return "driver_load";
                case SysEventCode.ImageLoad:
                    return "image_load";
                case SysEventCode.FileEvent:
                    return "file_event";
                case SysEventCode.CreateStreamHash:
                    return "create_stream_hash";
                case SysEventCode.FileDelete:
                    return "file_delete";
                case SysEventCode.RegistryAdd:
                    return "registry_add";
                case SysEventCode.RegistryDelete:
                    return "registry_delete";
                case SysEventCode.RegistrySet:
                    return "registry_set";
                default:
                    return "unknown_event";
            }
        }
    }

}
