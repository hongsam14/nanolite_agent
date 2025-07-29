// <copyright file="SysmonEventCodeExtension.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using Nanolite_agent.Beacon.SystemActivity;

    /// <summary>
    /// Provides extension methods for the <see cref="SysEventCode"/> enumeration.
    /// </summary>
    /// <remarks>This class includes methods to convert <see cref="SysEventCode"/> values to more readable
    /// string representations.</remarks>
    public static class SysEventCodeExtension
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
                case SysEventCode.CreateRemoteThread:
                    return "create_remote_thread";
                case SysEventCode.NetworkConnection:
                    return "network_connection";
                case SysEventCode.DnsQuery:
                    return "dns_query";
                case SysEventCode.DriverLoad:
                    return "driver_load";
                case SysEventCode.ImageLoad:
                    return "image_load";
                case SysEventCode.FileCreate:
                    return "file_create";
                case SysEventCode.FileModified:
                    return "file_modified";
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
                case SysEventCode.RegistryRename:
                    return "registry_rename";
                default:
                    return "unknown_event";
            }
        }

        /// <summary>
        /// Converts a <see cref="SysEventCode"/> to its corresponding <see cref="ActorType"/>.
        /// </summary>
        /// <param name="code">The system event code to convert.</param>
        /// <returns>The <see cref="ActorType"/> that corresponds to the specified <paramref name="code"/>. Returns <see
        /// cref="ActorType.Undefined"/> if the event code does not match any predefined actor type.</returns>
        public static ActorType ToActorType(this SysEventCode code)
        {
            switch (code)
            {
                case SysEventCode.ProcessCreation:
                case SysEventCode.ProcessTerminated:
                    return ActorType.NOT_ACTOR;
                case SysEventCode.ProcessAccess:
                case SysEventCode.CreateRemoteThread:
                    return ActorType.REMOTE_THREAD;
                case SysEventCode.ProcessTampering:
                    return ActorType.TAMPERING;
                case SysEventCode.NetworkConnection:
                case SysEventCode.DnsQuery:
                    return ActorType.CONNECT;
                case SysEventCode.DriverLoad:
                case SysEventCode.ImageLoad:
                    return ActorType.LOAD;
                case SysEventCode.FileCreate:
                    return ActorType.CREATE;
                case SysEventCode.FileDelete:
                    return ActorType.DELETE;
                case SysEventCode.FileModified:
                    return ActorType.MODIFY;
                case SysEventCode.CreateStreamHash:
                    return ActorType.CREATE_STREAM_HASH;
                case SysEventCode.RegistryAdd:
                    return ActorType.REG_ADD;
                case SysEventCode.RegistryDelete:
                    return ActorType.REG_DELETE;
                case SysEventCode.RegistrySet:
                    return ActorType.REG_SET;
                case SysEventCode.RegistryRename:
                    return ActorType.REG_RENAME;
                default:
                    return ActorType.Undefined;
            }
        }

        /// <summary>
        /// Converts a <see cref="SysEventCode"/> to its corresponding <see cref="ArtifectType"/>.
        /// </summary>
        /// <param name="code">The system event code to convert.</param>
        /// <returns>The <see cref="ArtifectType"/> that corresponds to the specified <paramref name="code"/>. Returns <see
        /// cref="ArtifectType.Undefined"/> if the event code does not match any known type.</returns>
        public static ArtifectType ToArtifectType(this SysEventCode code)
        {
            switch (code)
            {
                case SysEventCode.ProcessCreation:
                case SysEventCode.ProcessAccess:
                case SysEventCode.ProcessTerminated:
                case SysEventCode.CreateRemoteThread:
                case SysEventCode.ProcessTampering:
                    return ArtifectType.Process;
                case SysEventCode.NetworkConnection:
                case SysEventCode.DnsQuery:
                    return ArtifectType.Network;
                case SysEventCode.DriverLoad:
                case SysEventCode.ImageLoad:
                    return ArtifectType.Module;
                case SysEventCode.FileCreate:
                case SysEventCode.FileModified:
                case SysEventCode.FileDelete:
                case SysEventCode.CreateStreamHash:
                    return ArtifectType.File;
                case SysEventCode.RegistryAdd:
                case SysEventCode.RegistryDelete:
                case SysEventCode.RegistrySet:
                case SysEventCode.RegistryRename:
                    return ArtifectType.Registry;
                default:
                    return ArtifectType.Undefined;
            }
        }
    }
}
