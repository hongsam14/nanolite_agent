// <copyright file="ProcessActivity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon.SystemActivity
{
    using System;
    using System.Diagnostics;
    using nanolite_agent.Properties;

    /// <summary>
    /// Provides context for processing activities within a system, including managing actor activities and associating
    /// them with a process context.
    /// </summary>
    /// <remarks>This class is used to initialize and manage activities related to a specific process,
    /// allowing for the upsertion of actor behaviors and maintaining the association with the process
    /// context.</remarks>
    public class ProcessActivityContext
    {
        private readonly ActorActivitiesOfProcess rrActors;

        private readonly ActorActivitiesOfProcess wsActors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessActivityContext"/> class with the process activity
        /// sourceActivity, and process context.
        /// </summary>
        /// <param name="source">The <see cref="ActivitySource"/> used to create activities related to the process. Cannot be <see
        /// langword="null"/>.</param>
        /// <param name="processActivity">The <see cref="Activity"/> representing the current process activity. Cannot be <see langword="null"/>.</param>
        /// <param name="process">The <see cref="ProcessContext"/> associated with the process. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/>, <paramref name="processActivity"/>, or <paramref name="process"/> is
        /// <see langword="null"/>.</exception>
        public ProcessActivityContext(ActivitySource source, Activity processActivity, ProcessContext process)
        {
            if (source == null || processActivity == null || process == null)
            {
                throw new ArgumentNullException(nameof(source), DebugMessages.SystemActivityNullException);
            }

            this.Process = process;
            this.Activity = processActivity;

            this.rrActors = new ActorActivitiesOfProcess(source, ActorActivityType.READ_RECV);
            this.wsActors = new ActorActivitiesOfProcess(source, ActorActivityType.WRITE_SEND);
        }

        /// <summary>
        /// Gets the process context associated with this activity.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Gets the activity associated with this process context.
        /// </summary>
        public ProcessContext Process { get; private set; }

        /// <summary>
        /// Updates or inserts an activity based on the specified artifact and actor type.
        /// </summary>
        /// <remarks>The method distinguishes between read/receive and write/send actor activity types to
        /// determine the appropriate context for updating or inserting the actor's activity. If the actor type is not
        /// an actor, the method returns the current process activity and context without creating a new
        /// activity.</remarks>
        /// <param name="obj">The artifact associated with the actor activity. Cannot be <see langword="null"/>.</param>
        /// <param name="type">The type of the actor, which determines the activity context to be updated or inserted.</param>
        /// <returns>A tuple containing the updated or inserted <see cref="Activity"/> and the associated <see
        /// cref="ISystemContext"/>. If the actor type is not recognized as an actor, returns the current process
        /// activity and context.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
        /// <exception cref="NanoException.SystemActivityException">Thrown if the actor activity type derived from <paramref name="type"/> is unsupported.</exception>
        public (Activity, ISystemContext) UpsertActivity(Artifect obj, ActorType type)
        {
            ActorActivityType actorActivityType;
            ActorActivityContext actorActivityContext;

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), DebugMessages.SystemActivityNullException);
            }

            // check type is actor type
            actorActivityType = ActorTypeExtension.GetActorActivityTypeFromActorType(type);
            if (actorActivityType == ActorActivityType.NOT_ACTOR)
            {
                // This case means that the type is not actor type.
                // It's likely a process or thread activity.
                // In this case, we will not create a new activity for the actor.
                // just return the current ProcessActivity.
                // and add log count to process activity.
                return (this.Activity, this.Process);
            }

            // Determine the actor activity context based on the actor activity type
            switch (actorActivityType)
            {
                case ActorActivityType.READ_RECV:
                    actorActivityContext = this.rrActors.UpsertActor(this.Activity, obj, type);
                    break;
                case ActorActivityType.WRITE_SEND:
                    actorActivityContext = this.wsActors.UpsertActor(this.Activity, obj, type);
                    break;
                default:
                    throw new NanoException.SystemActivityException($"Unsupported actor activity type: {actorActivityType}");
            }

            return (actorActivityContext.Activity, actorActivityContext.Actor);
        }

        /// <summary>
        /// Flushes all actors in the read/receive and write/send contexts, releasing associated resources.
        /// </summary>
        /// <remarks>This method clears the current activity and process context, which may release
        /// resources and reset the state of the system. It should be called when the current processing cycle is
        /// complete and resources need to be freed.</remarks>
        public void Flush()
        {
            // Flush all actors in the read/receive and write/send contexts
            this.rrActors.FlushActors();
            this.wsActors.FlushActors();

            // Clear the activity and process context to release resources
            this.Activity = null;
            this.Process = null;
        }
    }
}
