// <copyright file="ProcessContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity.Context
{
    using System;
    using nanolite_agent.Properties;

    /// <summary>
    /// Represents the context of a process within the system, providing access to the associated artifact and managing
    /// process-specific information such as logging.
    /// </summary>
    /// <remarks>The <see cref="ProcessContext"/> class is used to encapsulate the state and behavior of a
    /// process that is launched within the system. It differs from <see cref="ActorContext"/> in that it specifically
    /// represents the process itself, rather than the actor that the process is associated with.</remarks>
    public class ProcessContext : ISystemContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContext"/> class with the process object artifect context.
        /// The difference between this and the <see cref="ActorContext"/> is that this context is used to represent the process that is launched,
        /// the other is used to represent the actor that process is behaviored.
        /// </summary>
        /// <param name="artifectContext">The artifect context to be used by the process. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="artifectContext"/> is <see langword="null"/>.</exception>
        public ProcessContext(Artifact artifectContext)
        {
            if (artifectContext == null)
            {
                throw new ArgumentNullException(nameof(artifectContext), DebugMessages.SystemActivityNullException);
            }

            // set artifect object
            this.ArtifectContext = artifectContext;

            // Initialize logCount to 0
            this.LogCount = 0;
        }

        /// <summary>
        /// Gets the artifect context associated with this process.
        /// </summary>
        public Artifact ArtifectContext { get; private set; }

        /// <summary>
        /// Gets the current log count for this process context.
        /// </summary>
        public int LogCount { get; private set; }

        /// <summary>
        /// Gets the context identifier for the current artifact.
        /// The format is "{ArtifectID}@LAUNCH".
        /// </summary>
        /// <remarks>The context ID is used to uniquely identify the process associated with the artifact
        /// within the system.</remarks>
        public string ContextID
        {
            get
            {
                if (this.ArtifectContext == null)
                {
                    return null;
                }

                // Return the context ID in the format "{ArtifectID}@TYPE"
                // ProcessContext uses "LAUNCH" as the type.
                // Because There are only Process type and Actor type in this context system.
                // And ProcessContext is used to represent the process that is launched.
                return $"{this.ArtifectContext.ArtifectID}@LAUNCH";
            }
        }

        /// <summary>
        /// Increments the log count by one.
        /// </summary>
        /// <remarks>This method increases the value of the <see cref="LogCount"/> property by one each
        /// time it is called.</remarks>
        public void IncrementLogCount()
        {
            // Increment the log count by one
            this.LogCount++;
        }
    }
}
