// <copyright file="SystemActivityType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon.SystemActivity
{
    /// <summary>
    /// ArtifectType enum represents Actor's artifect type.
    /// </summary>
    public enum ArtifectType
    {
        /// <summary>
        /// Represents an undefined state or value.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// File system artifect.
        /// </summary>
        File,

        /// <summary>
        /// Registry artifect.
        /// </summary>
        Registry,

        /// <summary>
        /// Network artifect.
        /// </summary>
        Network,

        /// <summary>
        ///  Process artifect. This is not Process Object. This is a type of artifect that represents a process injection.
        /// </summary>
        Process,

        /// <summary>
        /// Module artifect
        /// </summary>
        Module,
    }

    /// <summary>
    /// ActorType enum represents the type of actor in the system.
    /// </summary>
    public enum ActorType
    {
        /// <summary>
        /// Represents an undefined actor type.
        /// </summary>
        Undefined = 0,

        /************************************************************************
        * Process injection related actors
        * ***********************************************************************/

        /// <summary>
        /// Remote thread actor. This actor represents sysmon-event-8
        /// </summary>
        REMOTE_THREAD,

        /// <summary>
        /// Tampering actor. This actor represents sysmon-event-25
        /// </summary>
        TAMPERING,

        /************************************************************************
         * Network related actors
         * **********************************************************************/

        /// <summary>
        /// Connect actor. This actor represents sysmon-event-3
        /// </summary>
        CONNECT,

        /// <summary>
        /// Accept actor. This actor is not related to sysmon-event. This actor represents a network connection that is accepted by the system.
        /// </summary>
        ACCEPT,

        /************************************************************************
         * File system related actors
         * **********************************************************************/

        /// <summary>
        /// Create actor. This actor represents file-creation events such as sysmon-event-11, 29.
        /// </summary>
        CREATE,

        /// <summary>
        /// Delete actor. This actor represents file-deletion events such as sysmon-event-23.
        /// </summary>
        DELETE,

        /// <summary>
        /// Modify actor. This actor represents file-modification events such as sysmon-event-2
        /// </summary>
        MODIFY,

        /// <summary>
        /// Create stream hash actor. This actor represents sysmon-event-15.
        /// </summary>
        CREATE_STREAM_HASH,

        /* **********************************************************************
         * Registry related actors
         * **********************************************************************/

        /// <summary>
        /// Registry add actor. This actor represents sysmon-event-12.
        /// </summary>
        REG_ADD,

        /// <summary>
        /// Registry delete actor. This actor represents sysmon-event-12.
        /// </summary>
        REG_DELETE,

        /// <summary>
        /// Registry set actor. This actor represents sysmon-event-13.
        /// </summary>
        REG_SET,

        /// <summary>
        /// Registry rename actor. This actor represents sysmon-event-14.
        /// </summary>
        REG_RENAME,
    }

    /// <summary>
    /// ActorActivityType enum represents the type of activity an actor can perform in the system.
    /// There are two main types of activities: read/recv and write/send.
    /// read/recv activities involve retrieving or receiving data, while write/send activities involve sending or modifying data.
    /// write/send activities can include actions like tampering, connecting, creating, deleting, modifying, and registry operations.
    /// </summary>
    public enum ActorActivityType
    {
        /// <summary>
        /// Represents an undefined state or value.
        /// </summary>
        UNDEFINED = 0,

        /// <summary>
        /// Represents a read/recv operation such as network recv or file read, create_remote_thread, etc.
        /// </summary>
        READ_RECV,

        /// <summary>
        /// Represents a write/send operation such as network send or file write, tampering, etc.
        /// </summary>
        WRITE_SEND,
    }
}
