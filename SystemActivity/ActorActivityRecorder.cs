// <copyright file="ActorActivityRecorder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Nanolite_agent.SystemActivity.Context;

    /// <summary>
    /// Provides functionality to record and manage actor activities within a process, enabling tracing and monitoring
    /// of actor-related operations.
    /// </summary>
    /// <remarks>The <see cref="ActorActivityRecorder"/> class is designed to manage the lifecycle of actor
    /// activities, including creating, updating, and flushing actor contexts.  It ensures that actor activities are
    /// properly tracked and their data is sent to the collector when necessary. This class is particularly useful for
    /// tracing  distributed systems or monitoring actor-based workflows.</remarks>
    public class ActorActivityRecorder
    {
        private readonly ActorActivityType activityType;

        private readonly ActivitySource activitySource;

        private ConcurrentDictionary<string, ActorActivityContext> actorMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorActivityRecorder"/> class with the specified activity
        /// source and actor activity type.
        /// This object will hold the actor activity and context to trace process related activity.
        /// </summary>
        /// <param name="source">The source of the activity. Cannot be <see langword="null"/>.</param>
        /// <param name="activityType">The type of actor activity. Must not be <see cref="ActorActivityType.UNDEFINED"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/> or <paramref name="activityType"/> is <see
        /// cref="ActorActivityType.UNDEFINED"/>.</exception>
        public ActorActivityRecorder(in ActivitySource source, in ActorActivityType activityType)
        {
            // check activityType is defined
            if (source == null || activityType == ActorActivityType.UNDEFINED)
            {
                throw new ArgumentNullException(nameof(activityType), DebugMessages.SystemActivityNullException);
            }

            this.activitySource = source;
            this.activityType = activityType;
            this.actorMap = new ConcurrentDictionary<string, ActorActivityContext>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ActorActivityRecorder"/> class.
        /// </summary>
        /// <remarks>This destructor iterates over the collection of <see cref="ActorActivityContext"/>
        /// objects, ensuring that any ongoing activities are stopped and their data is sent to the collector. It also
        /// clears the internal map to release resources for garbage collection.</remarks>
        ~ActorActivityRecorder()
        {
            // Call FlushActors to ensure all activities are stopped and data is sent to the collector
            this.FlushActors();
        }

        /// <summary>
        /// Flushes all active actors, sending their activity data to the collector and releasing resources.
        /// </summary>
        /// <remarks>This method iterates over all actors in the current map, stops their activities to
        /// send data to the collector, and then clears the map to release resources. If the actor map is already null,
        /// the method returns immediately.</remarks>
        public void FlushActors()
        {
            // Check if actorMap is already null
            if (this.actorMap == null)
            {
                return;
            }

            // Iterate over the Values collection to get ActorActivityContext objects directly
            foreach (ActorActivityContext actorActivityContext in this.actorMap.Values)
            {
                // Dispose or stop the activity to send the data to the collector
                if (actorActivityContext.Activity != null)
                {
                    // set the log.count to activity tag
                    actorActivityContext.Activity.SetTag("log.count", actorActivityContext.Actor.LogCount);
                    actorActivityContext.Activity.SetTag("parent.context", actorActivityContext.Actor.ParentContextID ?? string.Empty);

                    // Stop the activity to send the data to the collector
                    actorActivityContext.Activity.Stop();
                }
            }

            // clear the actorMap and set null to release to garbage collector.
            this.actorMap.Clear();
            this.actorMap = null;
        }

        /// <summary>
        /// Inserts or updates an actor in the activity context map.
        /// </summary>
        /// <remarks>This method ensures that the actor's activity context is up-to-date in the internal
        /// map. If the actor does not exist, it creates a new activity context and adds it to the map.</remarks>
        /// <param name="processActivity">The activity associated with the current process. Cannot be null.</param>
        /// <param name="processContext">The context of the current process. Cannot be null.</param>
        /// <param name="artifect">The artifect representing the actor's context. Cannot be null.</param>
        /// <param name="type">The type of the actor. Must not be <see cref="ActorType.Undefined"/>.</param>
        /// <returns>An <see cref="ActorActivityContext"/> representing the actor's activity context. If the actor already
        /// exists, returns the existing context; otherwise, creates a new one.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processActivity"/> or <paramref name="artifect"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is <see cref="ActorType.Undefined"/> or does not match the expected
        /// activity type.</exception>
        /// <exception cref="NanoException.SystemActivityException">Thrown if a new activity cannot be created for the actor.</exception>
        public ActorActivityContext UpsertActor(Activity processActivity, ProcessContext processContext, Artifact artifect, ActorType type)
        {
            // null checfor artifect and type
            ArgumentNullException.ThrowIfNull(processActivity);
            ArgumentNullException.ThrowIfNull(processContext);
            ArgumentNullException.ThrowIfNull(artifect);
            if (type == ActorType.UNDEFINED)
            {
                throw new ArgumentNullException(nameof(artifect), DebugMessages.SystemActivityNullException);
            }

            // check actor type.
            if (type.GetActorActivityTypeFromActorType() != this.activityType)
            {
                throw new ArgumentException(DebugMessages.SystemActivityInvalidType, nameof(type));
            }

            // check if actorMap is null
            if (this.actorMap == null)
            {
                // raise exception if actorMap is null. because it means FlushActors has been called
                throw new NanoException.SystemActivityException(DebugMessages.SystemActivityInvalidOperationException
                    + "Upsert function is called after FlushActor");
            }

            // create ActorContext
            ActorActivityContext actorActivityContext = null;
            ActorContext newActor = new ActorContext(artifect, type, processContext.ArtifactContext);

            // Create ObjectActorContext if it does not exist
            if (this.actorMap.TryGetValue(newActor.ContextID, out ActorActivityContext value))
            {
                // get existing ActorActivityContext for Getter
                actorActivityContext = value;
            }
            else
            {
                // create new actor activity context
                actorActivityContext = new ActorActivityContext(this.activitySource, processActivity, newActor, this.activityType);

                // Add the new ActorActivityContext to the GetterSpan dictionary
                this.actorMap.TryAdd(newActor.ContextID, actorActivityContext);

                // Set tags for the activity
                ActorTypeExtension.TagActorActivityAttribute(actorActivityContext.Activity, this.activityType);

                // Start the activity
                actorActivityContext.Activity.Start();
            }

            return actorActivityContext;
        }

        /// <summary>
        /// Determines whether the specified artifact exists for the given actor type.
        /// </summary>
        /// <param name="artifact">The artifact to check for existence.</param>
        /// <param name="type">The type of actor associated with the artifact.</param>
        /// <returns><see langword="true"/> if the artifact exists for the specified actor type; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsArtifactExists(Artifact artifact, ActorType type)
        {
            if (this.actorMap == null)
            {
                return false;
            }

            return this.actorMap.ContainsKey(ISystemContext.GenerateActorContextID(artifact, type));
        }
    }
}
