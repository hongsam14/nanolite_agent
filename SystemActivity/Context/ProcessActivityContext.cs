// <copyright file="ProcessActivityContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity.Context
{
    using System;
    using System.Diagnostics;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Microsoft.Diagnostics.Tracing.Parsers.Clr;

    /// <summary>
    /// Provides context for processing activities within a system, including managing actor activities and associating
    /// them with a process context.
    /// </summary>
    /// <remarks>This class is used to initialize and manage activities related to a specific process,
    /// allowing for the upsertion of actor behaviors and maintaining the association with the process
    /// context.</remarks>
    public class ProcessActivityContext
    {
        private readonly ActorActivityRecorder rrActors;

        private readonly ActorActivityRecorder wsActors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessActivityContext"/> class, representing the context for a
        /// process activity, including its associated telemetry and metadata.
        /// </summary>
        /// <remarks>This constructor initializes a new activity for the process, setting its metadata and
        /// tags based on the provided parameters. If a parent process context is provided, the new activity will
        /// inherit its context. Additionally, this constructor initializes actor activity recorders for read/receive
        /// and write/send operations.</remarks>
        /// <param name="image">The name or identifier of the process image (e.g., the executable name) to associate with the activity.</param>
        /// <param name="source">The <see cref="ActivitySource"/> used to create and manage the activity for this process context. Cannot be
        /// <see langword="null"/>.</param>
        /// <param name="parentProcessContext">The parent <see cref="ProcessActivityContext"/> to establish a hierarchical relationship for the activity.
        /// If <see langword="null"/>, this instance will be treated as the root process context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public ProcessActivityContext(string image, in ActivitySource source, in ProcessActivityContext parentProcessContext)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), DebugMessages.SystemActivityNullException);
            }

            if (parentProcessContext == null)
            {
                // This case means that this processActivityContext is the root process context.
                // Create a new Activity for the process
                Activity.Current = null;
                this.Activity = source.CreateActivity(
                    "uninitializedProcess",
                    ActivityKind.Internal,
                    null);
            }
            else
            {
                Activity.Current = parentProcessContext.Activity;
                this.Activity = source.CreateActivity(
                    "uninitializedProcess",
                    ActivityKind.Internal,
                    parentProcessContext.Activity.Context);
            }

            // set the activity type to NOT_ACTOR.
            // This means that this context is not specifically an actor context.
            // because actor context is used to represent the actor that process is behaviored and influence on Artifacts.
            this.ActivityType = ActorActivityType.NOT_ACTOR;

            // start activity to generate a span
            // Important: this will generate a span for the process activity
            // This sequence is important because process object activity's name is set with the span ID
            // and if we don't start the activity, the span ID will not be generated.
            // and the process context will not be set correctly.
            this.Activity.Start();

            // create a new Artifact for the process
            //Artifact procArtifact = new Artifact(ArtifactType.PROCESS, this.Activity.SpanId.ToString());
            if (parentProcessContext == null)
            {
                Artifact procArtifact = new Artifact(ArtifactType.PROCESS, image);
                this.Process = new ProcessContext(procArtifact);
            }
            else
            {
                Artifact procArtifact = new Artifact(ArtifactType.PROCESS, image);
                this.Process = new ProcessContext(procArtifact, parentProcessContext.Process.ArtifactContext);
            }

            // set real name of activity
            this.Activity.DisplayName = this.ContextID;

            // set the otel tags for the activity
            this.Activity.SetTag("process.name", image);
            this.Activity.SetTag("act.type", "launch");

            this.rrActors = new ActorActivityRecorder(source, ActorActivityType.READ_RECV);
            this.wsActors = new ActorActivityRecorder(source, ActorActivityType.WRITE_SEND);
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
        /// Gets the type of activity performed by the actor in this process context.
        /// The value is fixed to the 'NOT_ACTOR' type, indicating that this context is not specifically.
        /// </summary>
        public ActorActivityType ActivityType { get; private set; }

        /// <summary>
        /// Gets the unique context ID for this process activity.
        /// </summary>
        public string ContextID
        {
            get
            {
                return $"{this.Process.ContextID}@{this.ActivityType}";
            }
        }

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
        public (Activity, ISystemContext) UpsertActivity(Artifact obj, ActorType type)
        {
            ActorActivityType actorActivityType;
            ActorActivityContext actorActivityContext;

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), DebugMessages.SystemActivityNullException);
            }

            // check type is actor type
            actorActivityType = type.GetActorActivityTypeFromActorType();
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
                    actorActivityContext = this.rrActors.UpsertActor(this.Activity, this.Process, obj, type);
                    break;
                case ActorActivityType.WRITE_SEND:
                    actorActivityContext = this.wsActors.UpsertActor(this.Activity, this.Process, obj, type);
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

            // stop the process activity
            if (this.Activity != null)
            {
                // set tag of log count
                this.Activity.SetTag("log.count", this.Process.LogCount);
                this.Activity.SetTag("parent.context", this.Process.ParentContextID);
                this.Activity.Stop();
            }

            // Clear the activity and process context to release resources
            this.Activity = null;
            this.Process = null;
        }


    }
}
