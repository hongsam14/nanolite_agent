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
        /// Represents the event code for thread start.
        /// </summary>
        ThreadStart,

        /// <summary>
        /// Represents the event code for process access.
        /// </summary>
        ProcessAccess,

        /// <summary>
        /// Represents the event code for process termination.
        /// </summary>
        ProcessTerminated,

        /// <summary>
        /// Represents the event code for creating a remote thread.
        /// </summary>
        CreateRemoteThread,

        /// <summary>
        /// Represents the event code for process tampering.
        /// </summary>
        ProcessTampering,

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
        /// Represents the event code for a network connection detected.
        /// </summary>
        RawAccessReadDetected,

        /// <summary>
        /// Represents the event code for a file create.
        /// </summary>
        FileCreate,

        /// <summary>
        /// Represents the event code for a file modification.
        /// </summary>
        FileModified,

        /// <summary>
        /// Represents the event code for file deletion.
        /// </summary>
        FileDelete,

        /// <summary>
        /// Represents the event code for creating a stream hash.
        /// </summary>
        CreateStreamHash,

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

        /// <summary>
        /// Represents the event code for renaming a registry entry.
        /// </summary>
        RegistryRename,

        /// <summary>
        /// Represents a query to retrieve information from the Windows Registry.
        /// </summary>
        /// <remarks>This class provides functionality to query specific keys and values in the Windows
        /// Registry. It is designed to simplify access to registry data for applications that require configuration or
        /// system information stored in the registry.</remarks>
        RegistryQuery,
    }
}
