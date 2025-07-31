// <copyright file="SystemActivityBeacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Beacon.SystemActivity;
    using Nanolite_agent.Helper;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// Provides functionality and observility to monitor and log system activities using OpenTelemetry and OCSF Schema.
    /// check https://schema.ocsf.io.
    /// </summary>
    /// <remarks>The <see cref="SystemActivityBeacon"/> class is responsible for initializing and managing
    /// OpenTelemetry tracing and logging for system activities. It uses configuration settings provided in a YAML file
    /// to set up the necessary exporters and processors. This class supports starting and stopping monitoring, as well
    /// as processing various types of system events such as process creation, termination, and network
    /// activities.</remarks>
    public sealed class SystemActivityBeacon
    {
        private readonly string beaconName = "system_activity_beacon";

        // log exporter and processor
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<SystemActivityBeacon> logger;

        private readonly OtlpLogExporter logExporter;

        // trace provider, processor and span source
        private readonly TracerProvider tracerProvider;

        private readonly ResourceBuilder resource;

        private readonly ProcessActivitiesOfSystem processActivities;

        // flag to check if beacon is running
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityBeacon"/> class, which sets up telemetry and
        /// logging for system activity monitoring using OpenTelemetry.
        /// </summary>
        /// <remarks>This constructor performs the following operations: <list type="bullet">
        /// <item><description>Performs a health check on the OpenTelemetry collector to ensure it is
        /// reachable.</description></item> <item><description>Configures OpenTelemetry tracing and logging using the
        /// provided configuration.</description></item> <item><description>Initializes the <see
        /// cref="ProcessActivitiesOfSystem"/> component for monitoring system activities.</description></item> </list>
        /// Ensure that the provided configuration contains valid values for the collector IP, port, and exporter
        /// name.</remarks>
        /// <param name="config">The configuration object containing settings required to initialize the beacon, such as the collector IP,
        /// port, and exporter name. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="NanoException.BeaconException">Thrown if the beacon initialization fails, such as during health checks, resource setup, or telemetry
        /// configuration.</exception>
        public SystemActivityBeacon(in Nanolite_agent.Config.Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string serviceName = this.beaconName;
            this.isRunning = false;

            // init Otel traceProvider
            try
            {
                // health check for the beacon
                try
                {
                    HttpClient httpClient = new HttpClient();

                    var response = httpClient.GetAsync($"http://{config.CollectorIP}:13133/health").GetAwaiter().GetResult();
                    Console.WriteLine($"Beacon health check response: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    throw new NanoException.BeaconException("Beacon health check failed.", ex);
                }

                this.resource = ResourceBuilder.CreateDefault().AddService(serviceName);

                OtlpExporterOptions option = new OtlpExporterOptions
                {
                    // check config is valid.
                    Endpoint = new Uri($"http://{config.CollectorIP}:{config.CollectorPort}"),
                    Protocol = OtlpExportProtocol.Grpc,
                };

                // initialize traceProvider with OtlpTraceExporter & BatchActivityExportProcessor
                OtlpTraceExporter traceExporter = new OtlpTraceExporter(option);
                BatchActivityExportProcessor traceProcessor = new BatchActivityExportProcessor(traceExporter);

                this.tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(this.resource)
                    .AddSource(config.Exporter)
                    .AddProcessor(traceProcessor)
                    .Build();

                // initialize log Exporter with OtlpLogExporter & BatchLogExportProcessor
                this.logExporter = new OtlpLogExporter(option);
                BatchLogRecordExportProcessor logProcessor = new BatchLogRecordExportProcessor(this.logExporter);

                this.loggerFactory = LoggerFactory.Create(loggingBuilder =>
                {
                    loggingBuilder.AddOpenTelemetry(options =>
                    {
                        options.IncludeScopes = false;
                        options.IncludeFormattedMessage = false;
                        options.ParseStateValues = false;

                        options.SetResourceBuilder(this.resource);
                        options.AddProcessor(logProcessor);
                    });
                });
                this.logger = this.loggerFactory.CreateLogger<Nanolite_agent.Beacon.SystemActivityBeacon>();
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            try
            {
                // init Otel span source.
                ActivitySource spanSource = new ActivitySource(config.Exporter);

                // initialize ProcessActivitiesOfSystem
                this.processActivities = new ProcessActivitiesOfSystem(this.logger, spanSource);
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }
        }

        /// <summary>
        /// Starts monitoring the beacon.
        /// </summary>
        /// <remarks>This method sets the beacon to a running state. If the beacon is already running,  an
        /// exception is thrown. Ensure that the beacon is not running before calling this method.</remarks>
        /// <exception cref="NanoException.BeaconException">Thrown if the beacon is already running.</exception>
        public void StartMonitoring()
        {
            if (this.isRunning)
            {
                throw new NanoException.BeaconException("Beacon is already running.");
            }

            // set running flag
            this.isRunning = true;
        }

        /// <summary>
        /// Stops the monitoring process and releases associated resources.
        /// </summary>
        /// <remarks>This method halts the monitoring process if it is currently running. It ensures that
        /// all  pending activities are flushed, and any associated resources, such as log exporters,  trace providers,
        /// and log processors, are properly disposed of.</remarks>
        /// <exception cref="NanoException.BeaconException">Thrown if the monitoring process is not currently running.</exception>
        public void StopMonitoring()
        {
            if (!this.isRunning)
            {
                throw new NanoException.BeaconException("Beacon is not running.");
            }

            // flush Activity Dictionaries
            this.processActivities.Flush();

            // flush and shutdown Otel log exporter
            this.logExporter?.ForceFlush();
            this.logExporter?.Shutdown();

            // flush and shutdown Otel trace provider
            this.tracerProvider?.ForceFlush();

            // dispose Otel log processor
            this.loggerFactory.Dispose();
            this.tracerProvider?.Dispose();

            this.tracerProvider?.Shutdown();
        }

        /// <summary>
        /// Processes an event log to create a system object representing a process launch.
        /// </summary>
        /// <remarks>This method extracts process-related information from the provided event log and
        /// invokes the  <see cref="ProcessActivities.ProcessLaunch"/> method to handle the process launch. If the
        /// required  data is missing or invalid, the method exits without performing any action.</remarks>
        /// <param name="eventData">A <see cref="ProcessTraceData"/> object containing event log data. The object must
        /// contain the following keys:
        /// <list type="bullet"> <item> <description><c>ProcessID</c>: The ID of the process being
        /// launched.</description> </item> <item> <description><c>ParentID</c>: The ID of the parent
        /// process.</description> </item> <item> <description><c>ImageFileName</c>: The name of the executable file for
        /// the process.</description> </item> </list> If any of these keys are missing or invalid, the method will not
        /// perform any action.</param>
        /// <param name="eventDecoderFunc">A function that takes a <see cref="ProcessTraceData"/> object and returns a
        /// JObject representing the decoded event data.
        /// </param>
        public void CreateSystemObject(ProcessTraceData eventData, Func<ProcessTraceData, JObject> eventDecoderFunc)
        {
            this.processActivities.ProcessLaunch(
                eventData.ProcessID,
                eventData.ParentID,
                eventData.ImageFileName,
                eventDecoderFunc,
                eventData);
        }

        /// <summary>
        /// Processes an event log to terminate a system object representing a process.
        /// </summary>
        /// <remarks>
        /// This method extracts process-related information from the provided event log and
        /// invokes the <see cref="ProcessActivitiesOfSystem.ProcessTerminate"/> method to handle the process termination.
        /// </remarks>
        /// <param name="eventData">A <see cref="ProcessTraceData"/> object containing event log data for the process to terminate.</param>
        /// <param name="eventDecoderFunc">A function that takes a <see cref="ProcessTraceData"/> object and returns a <see cref="JObject"/> representing the decoded event data.</param>
        public void TerminateSystemObject(ProcessTraceData eventData, Func<ProcessTraceData, JObject> eventDecoderFunc)
        {
            this.processActivities.ProcessTerminate(
                eventData.ProcessID,
                eventDecoderFunc,
                eventData);
        }

        /// <summary>
        /// Processes a system activity event by decoding its details and performing the appropriate action.
        /// </summary>
        /// <remarks>This method handles various system activity events, such as process creation, file
        /// modifications, network connections, and registry changes. Depending on the <paramref name="eventCode"/>, it
        /// extracts relevant information from the <paramref name="eventData"/> and processes the activity accordingly.
        /// If the event code is unknown or not supported, the method performs no action.</remarks>
        /// <param name="eventCode">The code representing the type of system event to process.</param>
        /// <param name="eventData">The event data containing details about the system activity.</param>
        /// <param name="eventDecoderFunc">A function that decodes the <see cref="TraceEvent"/> into a <see cref="JObject"/> for further processing.</param>
        public void ConsumeSystemActivity(SysEventCode eventCode, TraceEvent eventData, Func<TraceEvent, JObject> eventDecoderFunc)
        {
            string target;
            long processId;

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                case SysEventCode.ProcessTampering:
                    target = eventData.PayloadByName("Image")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.ProcessTerminated:
                    return;
                case SysEventCode.ProcessAccess:
                case SysEventCode.CreateRemoteThread:
                    target = eventData.PayloadByName("TargetImage")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.ImageLoad:
                case SysEventCode.DriverLoad:
                    target = eventData.PayloadByName("ImageLoaded")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.NetworkConnection:
                    target = eventData.PayloadByName("DestinationIp")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.DnsQuery:
                    target = eventData.PayloadByName("QueryName")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RegistryAdd:
                case SysEventCode.RegistrySet:
                case SysEventCode.RegistryDelete:
                    target = eventData.PayloadByName("TargetObject")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RegistryRename:
                    target = eventData.PayloadByName("NewName")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.FileCreate:
                case SysEventCode.FileDelete:
                case SysEventCode.FileModified:
                case SysEventCode.CreateStreamHash:
                    target = eventData.PayloadByName("TargetFilename")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RawAccessReadDetected:
                    target = eventData.PayloadByName("Device")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.Unknown:
                default:
                    // Unknown event code, do nothing
                    return;
            }

            // Get the process ID from the event data
            if (eventData.PayloadByName("ProcessId") is long pid)
            {
                processId = pid;
            }
            else if (eventData.PayloadByName("ProcessId") is int pidInt)
            {
                processId = pidInt;
            }
            else
            {
                // If ProcessID is not available, we cannot process this event
                return;
            }

            this.processActivities.ProcessAction(processId, target, eventCode, eventDecoderFunc, eventData);
        }
    }
}
