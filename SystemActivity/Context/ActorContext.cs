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
        /// <param name="artifactContext">The type of artifect, indicating its name and type. </param>
        /// <param name="type">The type of actor, indicating it's type of actor. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="artifactContext"/> is <see langword="null"/>
        /// or if <paramref name="type"/> is <see cref="ActorType.Undefined"/>.
        /// </exception>
        public ActorContext(Artifact artifactContext, ActorType type, Artifact parentContext = null)
        {
            if (artifactContext == null || type == ActorType.UNDEFINED)
            {
                throw new ArgumentNullException(nameof(artifactContext), DebugMessages.SystemActivityNullException);
            }

            // set artifect object
            this.ArtifactContext = artifactContext;

            // set parent artifect object
            this.ParentContext = parentContext;

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
        public Artifact ArtifactContext { get; private set; }

        /// <summary>
        /// Gets the parent artifect context associated with this actor.
        /// </summary>
        public Artifact ParentContext { get; private set; }

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
                if (this.ArtifactContext == null || this.Type == ActorType.UNDEFINED)
                {
                    return null;
                }

                // Generate a unique ID for the actor based on its type and artifect object ID
                return ISystemContext.GenerateActorContextID(this.ArtifactContext, this.Type);
            }
        }

        /// <summary>
        /// Gets the unique identifier of the parent context, or <see langword="null"/> if no parent context exists.
        /// </summary>
        public string ParentContextID
        {
            get
            {
                if (this.ParentContext == null)
                {
                    return null;
                }

                // Generate a unique ID for the parent artifect object
                return ISystemContext.GenerateProcessContextID(this.ParentContext);
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
