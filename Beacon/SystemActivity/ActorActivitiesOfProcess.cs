// <copyright file="ActorActivitiesOfProcess.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon.SystemActivity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using nanolite_agent.Properties;

    /// <summary>
    /// ActorActivitiesOfProcess class is a collection of actor activity contexts that are associated with a process.
    /// </summary>
    public class ActorActivitiesOfProcess
    {
        private readonly ActorActivityType activityType;

        private readonly ActivitySource activitySource;

        private Dictionary<string, ActorActivityContext> actorMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorActivitiesOfProcess"/> class with the specified activity
        /// source and actor activity type.
        /// This object will hold the actor activity and context to trace process related activity.
        /// </summary>
        /// <param name="source">The source of the activity. Cannot be <see langword="null"/>.</param>
        /// <param name="activityType">The type of actor activity. Must not be <see cref="ActorActivityType.UNDEFINED"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/> or <paramref name="activityType"/> is <see
        /// cref="ActorActivityType.UNDEFINED"/>.</exception>
        public ActorActivitiesOfProcess(ActivitySource source, ActorActivityType activityType)
        {
            // check activityType is defined
            if (source == null || activityType == ActorActivityType.UNDEFINED)
            {
                throw new ArgumentNullException(nameof(activityType), DebugMessages.SystemActivityNullException);
            }

            this.activitySource = source;
            this.activityType = activityType;
            this.actorMap = new Dictionary<string, ActorActivityContext>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ActorActivitiesOfProcess"/> class.
        /// </summary>
        /// <remarks>This destructor iterates over the collection of <see cref="ActorActivityContext"/>
        /// objects, ensuring that any ongoing activities are stopped and their data is sent to the collector. It also
        /// clears the internal map to release resources for garbage collection.</remarks>
        ~ActorActivitiesOfProcess()
        {
            // Iterate over the Values collection to get ActorActivityContext objects directly
            foreach (ActorActivityContext actorActivityContext in this.actorMap.Values)
            {
                // Dispose or stop the activity to send the data to the collector
                if (actorActivityContext.Activity != null)
                {
                    // set the log.count to activity tag
                    actorActivityContext.Activity.SetTag("log.count", actorActivityContext.Actor.LogCount);

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
        /// <param name="artifect">The artifect representing the actor's context. Cannot be null.</param>
        /// <param name="type">The type of the actor. Must not be <see cref="ActorType.Undefined"/>.</param>
        /// <returns>An <see cref="ActorActivityContext"/> representing the actor's activity context. If the actor already
        /// exists, returns the existing context; otherwise, creates a new one.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processActivity"/> or <paramref name="artifect"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is <see cref="ActorType.Undefined"/> or does not match the expected
        /// activity type.</exception>
        /// <exception cref="NanoException.SystemActivityException">Thrown if a new activity cannot be created for the actor.</exception>
        public ActorActivityContext UpsertActor(Activity processActivity, Artifect artifect, ActorType type)
        {
            // null checfor artifect and type
            if (processActivity == null || artifect == null || type == ActorType.Undefined)
            {
                throw new ArgumentNullException(nameof(artifect), DebugMessages.SystemActivityNullException);
            }

            // check actor type.
            if (ActorTypeExtension.GetActorActivityTypeFromActorType(type) != this.activityType)
            {
                throw new ArgumentException(DebugMessages.SystemActivityInvalidType, nameof(type));
            }

            // create ActorContext
            ActorActivityContext actorActivityContext = null;
            ActorContext newActor = new ActorContext(artifect, type);

            // Create ObjectActorContext if it does not exist
            if (this.actorMap.TryGetValue(newActor.ActorContextID, out ActorActivityContext value))
            {
                // get existing ActorActivityContext for Getter
                actorActivityContext = value;
            }
            else
            {
                // Create a new ActorActivityContext for Getter
                Activity newActivity = this.activitySource.CreateActivity(newActor.ActorContextID, ActivityKind.Internal, processActivity.Context);
                if (newActivity == null)
                {
                    throw new NanoException.SystemActivityException(DebugMessages.SystemActivityUpsertException);
                }

                // create new actor activity context
                actorActivityContext = new ActorActivityContext(newActivity, newActor);

                // Add the new ActorActivityContext to the GetterSpan dictionary
                this.actorMap[newActor.ActorContextID] = actorActivityContext;

                // Set tags for the activity
                ActorTypeExtension.TagActorActivityAttribute(newActivity, this.activityType);
            }

            return actorActivityContext;
        }
    }
}
