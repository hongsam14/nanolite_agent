// <copyright file="SelfInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent
{
    using System;

    /// <summary>
    /// SelfInfo class of the Nanolite agent.
    /// SelfInfo class contains the information of the current nanolite_agent process.
    /// It is used at preFiltering.
    /// </summary>
    public static partial class SelfInfo
    {
        /// <summary>
        /// Gets PID is the process ID of the current nanolite_agent process.
        /// </summary>
        public static int PID { get; private set; }

        /// <summary>
        /// Gets ThreadID is the thread ID of the current nanolite_agent process.
        /// </summary>
        public static int ThreadID { get; private set; }

        /// <summary>
        /// Gets HostName is the host name of the current nanolite_agent process.
        /// </summary>
        public static string HostName { get; private set; }

        /// <summary>
        /// Gets UserName is the user name of the current nanolite_agent process.
        /// </summary>
        public static string UserName { get; private set; }
    }

    /// <summary>
    /// SelfInfo class of the Nanolite agent.
    /// SelfInfo class contains the information of the current nanolite_agent process.
    /// It is used at preFiltering.
    /// </summary>
    public static partial class SelfInfo
    {
        static SelfInfo()
        {
            PID = System.Diagnostics.Process.GetCurrentProcess().Id;
            ThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            HostName = System.Environment.MachineName;
            UserName = System.Environment.UserName;
        }

    }
}
