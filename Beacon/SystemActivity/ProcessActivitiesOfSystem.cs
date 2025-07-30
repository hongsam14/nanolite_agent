// <copyright file="ProcessActivitiesOfSystem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon.SystemActivity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Manages the lifecycle and logging of system process activities, including launching, terminating, and processing
    /// events.
    /// </summary>
    /// <remarks>This class provides methods to initiate, terminate, and process actions for system processes,
    /// maintaining an internal map of process activity contexts. It logs relevant information and updates activity
    /// contexts as processes are managed.</remarks>
    public class ProcessActivitiesOfSystem
    {
        private readonly ActivitySource source;

        private readonly ILogger<SystemActivityBeacon> logger;

        private Dictionary<long, ProcessActivityContext> processMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessActivitiesOfSystem"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record system activity events. Cannot be <see langword="null"/>.</param>
        /// <param name="source">The activity source for generating telemetry data. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="source"/> is <see langword="null"/>.</exception>
        public ProcessActivitiesOfSystem(in ILogger<SystemActivityBeacon> logger, in ActivitySource source)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(source);

            this.source = source;
            this.logger = logger;
            this.processMap = new Dictionary<long, ProcessActivityContext>();
        }

        /// <summary>
        /// Initiates and logs the launch of a process, creating or updating the associated activity context.
        /// </summary>
        /// <remarks>This method manages the creation or updating of a process activity context within the
        /// system. If the process already exists in the map, its activity context is updated. Otherwise, a new activity
        /// context is created, potentially linking it to a parent process if specified. The method logs the process
        /// launch and increments the log count in the system context.</remarks>
        /// <param name="processId">The unique identifier of the process being launched.</param>
        /// <param name="parentProcessId">The unique identifier of the parent process, if any.</param>
        /// <param name="image">The image name of the process. Cannot be null or empty.</param>
        /// <param name="log">The log information associated with the process launch. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="image"/> is null or empty, or if <paramref name="log"/> is null.</exception>
        public void ProcessLaunch(
            long processId,
            long parentProcessId,
            string image,
            JObject log)
        {
            Activity activity;
            ISystemContext sysContext;

            // check if image is null
            if (string.IsNullOrEmpty(image) || log == null)
            {
                throw new ArgumentNullException(DebugMessages.SystemActivityNullException);
            }

            // check if process is already in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                Artifect existingProcessArtifect = existActContext.Process.ArtifectContext;

                // upsert existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(existingProcessArtifect, ActorType.NOT_ACTOR);
            }
            else
            {
                // create a new artifect for the process
                Artifect procArtifect = new Artifect(ArtifectType.Process, image);
                ProcessContext procContext = new ProcessContext(procArtifect);

                // check if parent process exists
                if (this.processMap.TryGetValue(parentProcessId, out ProcessActivityContext parentProcessContext))
                {
                    // if the parent process exists, we will create a new child activity with parent context.
                    activity = this.source.CreateActivity(
                        procContext.ContextID,
                        ActivityKind.Internal,
                        parentProcessContext.Activity.Context);
                }
                else
                {
                    // create a new activity for the process without parent context
                    activity = this.source.CreateActivity(
                        procContext.ContextID,
                        ActivityKind.Internal,
                        new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded));
                }

                // create a new process activity context
                ProcessActivityContext actContext = new ProcessActivityContext(this.source, activity, procContext);

                // upsert process activity context
                (Activity, ISystemContext) result = actContext.UpsertActivity(procArtifect, ActorType.NOT_ACTOR);

                // set tag of activity
                activity.SetTag("tag", "launch");

                // start activity
                activity.Start();

                // append to processMap
                this.processMap[processId] = actContext;

                sysContext = result.Item2;
            }

            // print log information
            Activity.Current = activity;
            this.logger.LogInformation(log.ToString(Newtonsoft.Json.Formatting.None));

            // increment log count
            sysContext.IncrementLogCount();

            Console.WriteLine($"Process {processId} launched with image {image}.");
        }

        /// <summary>
        /// Terminates the specified process and logs the associated activity.
        /// </summary>
        /// <remarks>This method logs the activity associated with the specified process, increments the
        /// log count, and ensures that all activities are stopped before removing the process from the internal
        /// map.</remarks>
        /// <param name="processId">The unique identifier of the process to terminate.</param>
        /// <param name="log">The log information to be recorded. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="log"/> is <see langword="null"/>.</exception>
        public void ProcessTerminate(
            long processId,
            JObject log)
        {
            Activity activity;
            ISystemContext sysContext;

            // check if log is null
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log), DebugMessages.SystemActivityNullException);
            }

            // check if process is in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(existActContext.Process.ArtifectContext, ActorType.NOT_ACTOR);

                // print log information
                Activity.Current = activity;
                this.logger.LogInformation(log.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();

                // set tag of log count
                activity.SetTag("log.count", sysContext.LogCount);

                // flush actors to ensure all activities are stopped
                existActContext.Flush();

                // stop activity
                activity.Stop();

                // remove from processMap
                this.processMap.Remove(processId);
            }
        }

        /// <summary>
        /// Processes a system event log entry for a specified process and updates the activity context accordingly.
        /// </summary>
        /// <remarks>This method updates the activity context for the specified process if it exists in
        /// the process map. It logs the event information and increments the log count for the system
        /// context.</remarks>
        /// <param name="processId">The unique identifier of the process associated with the event.</param>
        /// <param name="target">The target entity related to the event, such as a file or network address.</param>
        /// <param name="sysmonCode">The system event code that indicates the type of event being processed.</param>
        /// <param name="log">The JSON object containing the log details of the event. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="log"/> is <see langword="null"/>.</exception>
        /// <exception cref="NanoException.SystemActivityException">Thrown if the <paramref name="sysmonCode"/> is unsupported and cannot be mapped to a valid actor type.</exception>
        public void ProcessAction(
            long processId,
            string target,
            SysEventCode sysmonCode,
            JObject log)
        {
            Activity activity;
            ISystemContext sysContext;
            ActorType actorType;

            // check if log is null
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log), DebugMessages.SystemActivityNullException);
            }

            actorType = SysEventCodeExtension.ToActorType(sysmonCode);
            if (actorType == ActorType.Undefined)
            {
                throw new NanoException.SystemActivityException($"Unsupported sysmon code: {sysmonCode}");
            }

            // check if process is in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                Artifect actArtifect = new Artifect(SysEventCodeExtension.ToArtifectType(sysmonCode), target);

                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(actArtifect, actorType);

                // print log information
                Activity.Current = activity;
                this.logger.LogInformation(log.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();

                Console.WriteLine($"Process {processId} action processed: {sysmonCode} on target {target}");
            }
        }

        /// <summary>
        /// Flushes all process activity contexts and clears the internal process map.
        /// </summary>
        /// <remarks>This method iterates over all process activity contexts in the internal map, invoking
        /// their <see cref="ProcessActivityContext.Flush"/> method. After flushing, it clears the map and sets it to
        /// null, allowing it to be collected by the garbage collector.</remarks>
        public void Flush()
        {
            // check if processMap is null
            if (this.processMap == null)
            {
                return;
            }

            // Iterate over the Values collection to get ProcessActivityContext objects directly
            foreach (ProcessActivityContext processActivityContext in this.processMap.Values)
            {
                processActivityContext.Flush();
            }

            // clear the processMap and set null to release to garbage collector.
            this.processMap.Clear();
            this.processMap = null;
        }
    }
}
