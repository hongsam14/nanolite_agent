// <copyright file="ISystemContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity.Context
{
    /// <summary>
    /// Represents a context within the system, providing access to specific artifacts and context-related information.
    /// </summary>
    /// <remarks>This interface defines properties and methods to interact with the system context, including
    /// retrieving artifact details, identifying the context, and managing log counts.</remarks>
    public interface ISystemContext
    {
        /// <summary>
        /// Gets the current context of the artifact.
        /// </summary>
        Artifact ArtifectContext { get; }

        /// <summary>
        /// Gets the unique identifier for the current context.
        /// </summary>
        string ContextID { get; }

        /// <summary>
        /// Gets the total number of log entries recorded.
        /// </summary>
        int LogCount { get; }

        /// <summary>
        /// Increments the internal log count by one.
        /// </summary>
        /// <remarks>This method updates the log count, which may be used to track the number of log
        /// entries processed. Ensure that the log count is initialized before calling this method to avoid unexpected
        /// results.</remarks>
        void IncrementLogCount();
    }
}
