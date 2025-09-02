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
        Artifact ArtifactContext { get; }

        /// <summary>
        /// Gets the parent context of the artifact.
        /// </summary>
        Artifact ParentContext { get; }

        /// <summary>
        /// Gets the unique identifier for the current context.
        /// </summary>
        string ContextID { get; }

        /// <summary>
        /// Gets the unique identifier for the parent context.
        /// </summary>
        string ParentContextID { get; }

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

        /// <summary>
        /// Generates a unique process context identifier for the specified artifact.
        /// ActorType is fixed to "LAUNCH" for process contexts.
        /// </summary>
        /// <param name="artifact">The artifact for which the process context identifier is generated. Must not be null.</param>
        /// <returns>A string representing the process context identifier, formatted as "<c>ArtifactID@LAUNCH</c>".</returns>
        static string GenerateProcessContextID(Artifact artifact) => $"{artifact.ArtifectID}@LAUNCH";

        /// <summary>
        /// Generates a unique identifier for an actor context based on the specified artifact and actor type.
        /// </summary>
        /// <param name="artifact">The artifact used to generate the context ID. Must not be null.</param>
        /// <param name="type">The type of the actor for which the context ID is being generated.</param>
        /// <returns>A string representing the unique actor context ID, formatted as "<c>ArtifactID@ActorType</c>".</returns>
        static string GenerateActorContextID(Artifact artifact, ActorType type) => $"{artifact.ArtifectID}@{type}";
    }
}
