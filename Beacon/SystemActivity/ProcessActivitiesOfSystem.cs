// <copyright file="ProcessActivitiesOfSystem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon.SystemActivity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
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
        /// Handles the launch of a process, creating or updating the process activity context,
        /// logging the event, and associating it with its parent if available.
        /// </summary>
        /// <param name="processId">The unique identifier of the process being launched.</param>
        /// <param name="parentProcessId">The unique identifier of the parent process.</param>
        /// <param name="image">The image name or path of the process.</param>
        /// <param name="eventDecoderFunc">A function to decode the trace event into a <see cref="JObject"/> log entry.</param>
        /// <param name="sysEvent">The system trace event associated with the process launch.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="image"/>, <paramref name="eventDecoderFunc"/>, or <paramref name="sysEvent"/> is <c>null</c>.
        /// </exception>
        public void ProcessLaunch(
            long processId,
            long parentProcessId,
            string image,
            Func<ProcessTraceData, JObject> eventDecoderFunc,
            ProcessTraceData sysEvent)
        {
            Activity activity;
            ISystemContext sysContext;
            JObject log;

            // check if image is null
            if (string.IsNullOrEmpty(image))
            {
                throw new ArgumentNullException(DebugMessages.SystemActivityNullException);
            }

            // check sysEvent or eventDecoderFunc is null
            ArgumentNullException.ThrowIfNull(eventDecoderFunc);
            ArgumentNullException.ThrowIfNull(sysEvent);

            // decode the sysEvent to get log information
            log = eventDecoderFunc(sysEvent);
            if (log == null)
            {
                // in this case, the log does not pass the filter so we will return
                return;
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
                    Activity.Current = parentProcessContext.Activity;

                    // if the parent process exists, we will create a new child activity with parent context.
                    activity = this.source.CreateActivity(
                        procContext.ContextID,
                        ActivityKind.Internal,
                        parentProcessContext.Activity.Context);
                }
                else
                {
                    // if the parent process does not exist, we will set the current activity to null.
                    Activity.Current = null;

                    // create a new activity for the process without parent context
                    activity = this.source.CreateActivity(
                        procContext.ContextID,
                        ActivityKind.Internal,
                        null);
                }

                // create a new process activity context
                ProcessActivityContext actContext = new ProcessActivityContext(this.source, activity, procContext);

                // upsert process activity context
                (Activity, ISystemContext) result = actContext.UpsertActivity(procArtifect, ActorType.NOT_ACTOR);

                // set tag of activity
                activity.SetTag("act.type", "launch");

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
        }

        /// <summary>
        /// Terminates the process associated with the specified process ID and logs relevant information.
        /// </summary>
        /// <remarks>If the decoded log returned by <paramref name="eventDecoderFunc"/> is <see
        /// langword="null"/>,  the method exits without performing any further actions. Otherwise, the method logs the
        /// decoded  information, updates the process activity context, and removes the process from the internal
        /// map.</remarks>
        /// <param name="processId">The unique identifier of the process to terminate.</param>
        /// <param name="eventDecoderFunc">A function that decodes a <see cref="ProcessTraceData"/> into a <see cref="JObject"/>.  This function must not
        /// return <see langword="null"/>.</param>
        /// <param name="sysEvent">The system event associated with the process termination. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="eventDecoderFunc"/>, or <paramref name="sysEvent"/> is <c>null</c>.
        /// </exception>
        public void ProcessTerminate(
            long processId,
            Func<ProcessTraceData, JObject> eventDecoderFunc,
            ProcessTraceData sysEvent)
        {
            Activity activity;
            ISystemContext sysContext;
            JObject log;

            // check if log is null
            ArgumentNullException.ThrowIfNull(eventDecoderFunc);
            ArgumentNullException.ThrowIfNull(sysEvent);

            log = eventDecoderFunc(sysEvent);
            if (log == null)
            {
                // in this case, the log does not pass the filter so we will return
                return;
            }

            // check if process is in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(existActContext.Process.ArtifectContext, ActorType.NOT_ACTOR);

                // send log information to otel collector
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
        /// Processes a system event and updates the associated process activity context.
        /// </summary>
        /// <remarks>This method processes a system event by decoding it into a log, determining the actor
        /// type based on the  system event code, and updating the associated process activity context. If the log does
        /// not pass the  filter (i.e., the decoder function returns <see langword="null"/>), the method exits without
        /// further processing.  If the process activity context exists, the method updates it with the event details,
        /// logs the information,  and increments the log count. The method also ensures that the activity is set for
        /// telemetry purposes.</remarks>
        /// <param name="processId">The unique identifier of the process associated with the event.</param>
        /// <param name="target">The target resource or entity related to the event.</param>
        /// <param name="sysmonCode">The system event code that categorizes the type of event.</param>
        /// <param name="eventDecoderFunc">A function that decodes the <see cref="TraceEvent"/> into a <see cref="JObject"/> log.  The function must
        /// not return <see langword="null"/>.</param>
        /// <param name="sysEvent">The system event to be processed. Cannot be <see langword="null"/>.</param>
        /// <exception cref="NanoException.SystemActivityException">Thrown if the <paramref name="sysmonCode"/> is unsupported or cannot be mapped to a valid actor type.</exception>
        public void ProcessAction(
            long processId,
            string target,
            SysEventCode sysmonCode,
            Func<TraceEvent, JObject> eventDecoderFunc,
            TraceEvent sysEvent)
        {
            Activity activity;
            ISystemContext sysContext;
            ActorType actorType;
            JObject log;

            // check if log is null
            ArgumentNullException.ThrowIfNull(eventDecoderFunc);
            ArgumentNullException.ThrowIfNull(sysEvent);

            log = eventDecoderFunc(sysEvent);
            if (log == null)
            {
                // in this case, the log does not pass the filter so we will return
                return;
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

                // send log information to otel collector
                Activity.Current = activity;

                this.logger.LogInformation(log.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();
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
