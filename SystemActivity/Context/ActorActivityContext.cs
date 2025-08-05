// <copyright file="ActorActivityContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity.Context
{
    using System;
    using System.Diagnostics;
    using nanolite_agent.Properties;

    /// <summary>
    /// ActorActivityContext class represents the context of an actor's activity.
    /// This Object contains the otel activity and the actor context.
    /// </summary>
    public class ActorActivityContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActorActivityContext"/> class with the specified activity and
        /// actor context.
        /// </summary>
        /// <param name="activitySource">The <see cref="ActivitySource"/> used to create the activity. Cannot be <see langword="null"/>.</param>
        /// <param name="processActivity"> The <see cref="Activity"/> representing the process activity. Cannot be <see langword="null"/>.</param>
        /// <param name="actor">The <see cref="ActorContext"/> associated with the actor. Cannot be <see langword="null"/>.</param>
        /// <param name="type">The type of activity performed by the actor, represented by <see cref="ActorActivityType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="activity"/> or <paramref name="actor"/> is <see langword="null"/>.</exception>
        public ActorActivityContext(ActivitySource activitySource, Activity processActivity, ActorContext actor, ActorActivityType type)
        {
            if (activitySource == null || processActivity == null || actor == null)
            {
                throw new ArgumentNullException(nameof(activitySource), DebugMessages.SystemActivityNullException);
            }

            this.Actor = actor;
            this.ActivityType = type;

            // Create a new ActorActivityContext for Getter
            Activity.Current = processActivity;
            this.Activity = activitySource.CreateActivity(this.ContextID, ActivityKind.Internal, processActivity.Context);
            if (this.Activity == null)
            {
                throw new NanoException.SystemActivityException(DebugMessages.SystemActivityUpsertException);
            }
        }

        /// <summary>
        /// Gets the activity associated with this actor context.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Gets the actor context associated with this activity.
        /// </summary>
        public ActorContext Actor { get; private set; }

        /// <summary>
        /// Gets the type of activity performed by the actor.
        /// </summary>
        public ActorActivityType ActivityType { get; private set; }

        /// <summary>
        /// Gets the unique context ID for this actor activity.
        /// </summary>
        public string ContextID
        {
            get
            {
                return $"{this.Actor.ContextID}@{this.ActivityType}";
            }
        }
    }
}
