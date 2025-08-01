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
        /// <param name="activity">The activity associated with the actor. Cannot be <see langword="null"/>.</param>
        /// <param name="actor">The context of the actor performing the activity. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="activity"/> or <paramref name="actor"/> is <see langword="null"/>.</exception>
        public ActorActivityContext(Activity activity, ActorContext actor)
        {
            if (activity == null || actor == null)
            {
                throw new ArgumentNullException(nameof(activity), DebugMessages.SystemActivityNullException);
            }

            this.Activity = activity;
            this.Actor = actor;
        }

        /// <summary>
        /// Gets the activity associated with this actor context.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Gets the actor context associated with this activity.
        /// </summary>
        public ActorContext Actor { get; private set; }
    }
}
