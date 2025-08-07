// <copyright file="SystemActivityRecorder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Nanolite_agent.SystemActivity.Context;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Record the lifecycle and logging of system process activities, including launching, terminating, and processing
    /// events.
    /// </summary>
    /// <remarks>This class provides methods to initiate, terminate, and process actions for system processes,
    /// maintaining an internal map of process activity contexts. It logs relevant information and updates activity
    /// contexts as processes are managed.</remarks>
    public class SystemActivityRecorder
    {
        private readonly ActivitySource source;

        private readonly ILogger<SystemActivityBeacon> logger;

        private ConcurrentDictionary<long, ProcessActivityContext> processMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityRecorder"/> class,  which processes and tracks
        /// system activities based on the provided activity beacon.
        /// </summary>
        /// <param name="beacon">The <see cref="SystemActivityBeacon"/> that provides the source of system activities  and the logger for
        /// tracking activity processing. Cannot be <see langword="null"/>.</param>
        public SystemActivityRecorder(in SystemActivityBeacon beacon)
        {
            ArgumentNullException.ThrowIfNull(beacon);
            ArgumentNullException.ThrowIfNull(beacon.Logger);
            ArgumentNullException.ThrowIfNull(beacon.SystemActivitySource);

            this.source = beacon.SystemActivitySource;
            this.logger = beacon.Logger;
            this.processMap = new ConcurrentDictionary<long, ProcessActivityContext>();
        }

        /// <summary>
        /// Start Recording the launch of a process, creating or updating the process activity context,
        /// logging the event, and associating it with its parent if available.
        /// </summary>
        /// <param name="processId">The unique identifier of the process being launched.</param>
        /// <param name="parentProcessId">The unique identifier of the parent process.</param>
        /// <param name="image">The image name or path of the process.</param>
        /// <param name="syslog">The system trace event associated with the process launch.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="image"/>, <paramref name="syslog"/> is <c>null</c>.
        /// </exception>
        public void StartRecordProcessObject(
            long processId,
            long parentProcessId,
            string image,
            JObject syslog)
        {
            Activity activity;
            ISystemContext sysContext;

            // check if image is null
            if (string.IsNullOrEmpty(image))
            {
                throw new ArgumentNullException(DebugMessages.SystemActivityNullException);
            }

            if (processId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processId), "Process ID must be greater than zero.");
            }

            if (parentProcessId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(parentProcessId), "Parent Process ID must be greater than or equal to zero.");
            }

            // check sysEvent or eventDecoderFunc is null
            ArgumentNullException.ThrowIfNull(syslog);

            // check if process is already in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                Artifact existingProcessArtifact = existActContext.Process.ArtifactContext;

                // upsert existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(existingProcessArtifact, ActorType.NOT_ACTOR);
            }
            else
            {
                ProcessActivityContext parentProcessContext = null;

                // check if parent process exists
                this.processMap.TryGetValue(parentProcessId, out parentProcessContext);

                // create a new process activity context
                ProcessActivityContext actContext = new ProcessActivityContext(image, this.source, parentProcessContext);

                // upsert process activity context
                (Activity, ISystemContext) result = actContext.UpsertActivity(actContext.Process.ArtifactContext, ActorType.NOT_ACTOR);

                // append to processMap
                this.processMap.TryAdd(processId, actContext);

                sysContext = result.Item2;
                activity = result.Item1;
            }

            // print log information
            Activity.Current = activity;

            this.logger.LogInformation(syslog.ToString(Newtonsoft.Json.Formatting.None));

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
        /// <param name="syslog">The system event associated with the process termination. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="syslog"/> is <c>null</c>.
        /// </exception>
        public void StopRecordProcessObject(
            long processId,
            JObject syslog)
        {
            Activity activity;
            ISystemContext sysContext;

            // check if log is null
            if (processId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processId), "Process ID must be greater than zero.");
            }

            ArgumentNullException.ThrowIfNull(syslog);

            // check if process is in the map
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(existActContext.Process.ArtifactContext, ActorType.NOT_ACTOR);

                // send log information to otel collector
                Activity.Current = activity;

                this.logger.LogInformation(syslog.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();

                // flush actors to ensure all activities are stopped
                existActContext.Flush();

                // stop activity
                activity.Stop();

                // remove from processMap
                this.processMap.TryRemove(processId, out _);
            }
        }

        /// <summary>
        /// Records a process access event, associating it with the specified process and target.
        /// </summary>
        /// <remarks>This method logs the process access event and updates the activity context for the
        /// specified process. If the target process is not found in the process map, the <paramref name="target"/>
        /// parameter is used as the target name. The method also increments the log count for the associated system
        /// context and sends the log information to the OpenTelemetry collector.</remarks>
        /// <param name="processId">The ID of the process initiating the access. Must be greater than zero.</param>
        /// <param name="targetProcessId">The ID of the target process being accessed. If not found in the process map, the <paramref name="target"/>
        /// parameter will be used as the target name.</param>
        /// <param name="target">The name of the target being accessed. Cannot be null or empty.</param>
        /// <param name="sysmonCode">The Sysmon event code representing the type of activity. Must not be <see cref="SysEventCode.Unknown"/> and
        /// must correspond to a tampering-related activity.</param>
        /// <param name="syslog">The JSON object containing the system log details for the event. Cannot be null.</param>
        /// <exception cref="NanoException.SystemActivityException">Thrown if <paramref name="sysmonCode"/> is <see cref="SysEventCode.Unknown"/> or if the event code does not
        /// correspond to a tampering-related activity.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="processId"/> is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null or empty, or if <paramref name="syslog"/> is null.</exception>
        public void RecordProcessAccess(
            long processId,
            long targetProcessId,
            string target,
            SysEventCode sysmonCode,
            JObject syslog)
        {
            Activity activity;
            ISystemContext sysContext;
            ActorType actorType;

            // check argument is valid
            if (sysmonCode == SysEventCode.Unknown)
            {
                throw new NanoException.SystemActivityException("Sysmon code cannot be undefined.");
            }

            if (processId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processId), "Process ID must be greater than zero.");
            }

            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target), DebugMessages.SystemActivityNullException);
            }

            // check sysLog is null
            ArgumentNullException.ThrowIfNull(syslog);

            actorType = sysmonCode.ToActorType();

            // check if sysmonCode is not tampering
            if (actorType != ActorType.REMOTE_THREAD)
            {
                throw new NanoException.SystemActivityException($"Unsupported sysmon code: {sysmonCode}");
            }

            // check if sysEvent is from the process which we are tracking with processMap
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                Artifact actArtifact;
                // create a new Artifact for the sysmon code
                // check targetProcessId is existing in processMap
                if (this.processMap.TryGetValue(targetProcessId, out ProcessActivityContext targetProcessContext))
                {
                    actArtifact = new Artifact(sysmonCode.ToArtifactType(), targetProcessContext.Process.ArtifactContext.ObjectName);
                }
                else
                {
                    // create a new Artifact for the sysmon code
                    // if targetProcessId is not in processMap, use target as the name
                    actArtifact = new Artifact(sysmonCode.ToArtifactType(), target);
                }

                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(actArtifact, actorType);

                // send log information to otel collector
                Activity.Current = activity;

                this.logger.LogInformation(syslog.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();
            }
        }

        /// <summary>
        /// Record a system event and updates the associated process activity context.
        /// </summary>
        /// <remarks>This method processes a system event by decoding it into a log, determining the actor
        /// type based on the  system event code, and updating the associated process activity context.
        /// If the process activity context exists, the method updates it with the event details,
        /// logs the information,  and increments the log count. The method also ensures that the activity is set for
        /// telemetry purposes.</remarks>
        /// <param name="processId">The unique identifier of the process associated with the event.</param>
        /// <param name="target">The target resource or entity related to the event.</param>
        /// <param name="sysmonCode">The system event code that categorizes the type of event.</param>
        /// <param name="syslog">The system event to be processed. Cannot be <see langword="null"/>.</param>
        /// <exception cref="NanoException.SystemActivityException">Thrown if the <paramref name="sysmonCode"/> is unsupported or cannot be mapped to a valid actor type.</exception>
        public void RecordProcessAction(
            long processId,
            string target,
            SysEventCode sysmonCode,
            JObject syslog)
        {
            Activity activity;
            ISystemContext sysContext;
            ActorType actorType;

            // check argument is valid
            if (sysmonCode == SysEventCode.Unknown)
            {
                throw new NanoException.SystemActivityException("Sysmon code cannot be undefined.");
            }

            if (processId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processId), "Process ID must be greater than zero.");
            }

            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target), DebugMessages.SystemActivityNullException);
            }

            // check sysLog is null
            ArgumentNullException.ThrowIfNull(syslog);

            actorType = sysmonCode.ToActorType();
            if (actorType == ActorType.UNDEFINED)
            {
                throw new NanoException.SystemActivityException($"Unsupported sysmon code: {sysmonCode}");
            }

            // check if sysEvent is from the process which we are tracking with processMap
            if (this.processMap.TryGetValue(processId, out ProcessActivityContext existActContext))
            {
                // create a new Artifact for the sysmon code
                Artifact actArtifact = new Artifact(sysmonCode.ToArtifactType(), target);

                // get existing process activity context
                (activity, sysContext) = existActContext.UpsertActivity(actArtifact, actorType);

                // send log information to otel collector
                Activity.Current = activity;

                this.logger.LogInformation(syslog.ToString(Newtonsoft.Json.Formatting.None));

                // increment log count
                sysContext.IncrementLogCount();
            }
        }

        /// <summary>
        /// Determines whether the specified process ID is being tracked.
        /// </summary>
        /// <param name="processId">The unique identifier of the process to check.</param>
        /// <returns><see langword="true"/> if the process with the specified ID is being tracked;  otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsProcessTracked(long processId)
        {
            // check if processMap is null
            if (this.processMap == null)
            {
                return false;
            }

            // check if process is in the map
            return this.processMap.ContainsKey(processId);
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
