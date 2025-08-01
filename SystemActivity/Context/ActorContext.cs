// <copyright file="ActorContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity.Context
{
    using System;
    using nanolite_agent.Properties;

    /// <summary>
    /// Represents the context of an actor within a system, encapsulating the actor's type and associated artifect
    /// context.
    /// </summary>
    /// <remarks>The <see cref="ActorContext"/> class is used to define the environment in which an actor
    /// operates,  distinguishing it from a <see cref="ProcessContext"/> by focusing on the actor's role and the
    /// artifect it interacts with.</remarks>
    public class ActorContext : ISystemContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActorContext"/> class with the specified artifect context and actor type.
        /// The difference between this and the <see cref="ProcessContext"/> is that this context is used to represent the actor that process is behaving,
        /// </summary>
        /// <param name="artifectContext">The type of artifect, indicating its name and type. </param>
        /// <param name="type">The type of actor, indicating it's type of actor. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="artifectContext"/> is <see langword="null"/>
        /// or if <paramref name="type"/> is <see cref="ActorType.Undefined"/>.
        /// </exception>
        public ActorContext(Artifact artifectContext, ActorType type)
        {
            if (artifectContext == null || type == ActorType.Undefined)
            {
                throw new ArgumentNullException(nameof(artifectContext), DebugMessages.SystemActivityNullException);
            }

            // set artifect object
            this.ArtifectContext = artifectContext;

            // set actor type
            this.Type = type;

            // Initialize LogCount to 0
            this.LogCount = 0;
        }

        /// <summary>
        /// Gets the type of the actor. This indicates the role or function of the actor in the system.
        /// <see cref="ActorType"/> is an enumeration that defines various types of actors, such as remote threads, tampering actors, etc."/>
        /// </summary>
        public ActorType Type { get; private set; }

        /// <summary>
        /// Gets the artifect context associated with this actor.
        /// </summary>
        public Artifact ArtifectContext { get; private set; }

        /// <summary>
        /// Gets the current log count for this actor context.
        /// </summary>
        public int LogCount { get; private set; }


        /// <summary>
        /// Gets the identifier for the actor context, which is a combination of the artifect ID and the actor type.
        /// The format is "{ArtifectID}@{ActorType}".
        /// </summary>
        public string ContextID
        {
            get
            {
                if (this.ArtifectContext == null || this.Type == ActorType.Undefined)
                {
                    return null;
                }

                // Generate a unique ID for the actor based on its type and artifect object ID
                return $"{this.ArtifectContext.ArtifectID}@{this.Type}";
            }
        }

        /// <summary>
        /// Increments the current log count by one.
        /// </summary>
        /// <remarks>This method increases the value of the <see cref="LogCount"/> property, which tracks
        /// the number of logs processed.</remarks>
        public void IncrementLogCount()
        {
            this.LogCount++;
        }
    }
}
