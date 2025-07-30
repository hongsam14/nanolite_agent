// <copyright file="SystemActivityBeacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
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

        // nano-agent config file
        private readonly Nanolite_agent.Config.Config config;

        private readonly ProcessActivitiesOfSystem processActivities;

        // flag to check if beacon is running
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityBeacon"/> class.
        /// </summary>
        /// <param name="config">config.yml file.</param>
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

            this.config = config;
        }

        public void StartMonitoring()
        {
            if (this.isRunning)
            {
                throw new NanoException.BeaconException("Beacon is already running.");
            }

            // set running flag
            this.isRunning = true;
        }

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

        public void CreateSystemObject(JObject eventlog)
        {
            long processId;
            long parentProcessId;
            string target;

            processId = eventlog.TryGetValue("ProcessID", out JToken processIdToken) ? (long)processIdToken : -1;
            parentProcessId = eventlog.TryGetValue("ParentID", out JToken parentProcessIdToken) ? (long)parentProcessIdToken : -1;
            target = eventlog.TryGetValue("ImageFileName", out JToken targetToken) ? targetToken.ToString() : string.Empty;

            if (processId < 0 || parentProcessId < 0 || string.IsNullOrEmpty(target))
            {
                return;
            }

            this.processActivities.ProcessLaunch(processId, parentProcessId, target, eventlog);
        }

        public void TerminateSystemObject(JObject eventlog)
        {
            long processId;

            processId = eventlog.TryGetValue("ProcessID", out JToken processIdToken) ? (long)processIdToken : -1;

            if (processId < 0)
            {
                return;
            }

            this.processActivities.ProcessTerminate(processId, eventlog);
        }

        public void ConsumeSystemActivity(SysEventCode eventCode, JObject eventlog)
        {
            long processId;
            string target;

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    long parentProcessId;
                    {
                        parentProcessId = eventlog.TryGetValue("ParentProcessId", out JToken parentProcessIdToken) ? (long)parentProcessIdToken : -1;
                        target = eventlog.TryGetValue("Image", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    if (parentProcessId < 0 || string.IsNullOrEmpty(target))
                    {
                        return;
                    }

                    break;
                case SysEventCode.ProcessTerminated:
                    return;
                case SysEventCode.ProcessTampering:
                    {
                        target = eventlog.TryGetValue("Image", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.ProcessAccess:
                case SysEventCode.CreateRemoteThread:
                    {
                        target = eventlog.TryGetValue("TargetImage", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.ImageLoad:
                case SysEventCode.DriverLoad:
                    {
                        target = eventlog.TryGetValue("ImageLoaded", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.NetworkConnection:
                    {
                        target = eventlog.TryGetValue("DestinationIp", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.DnsQuery:
                    {
                        target = eventlog.TryGetValue("QueryName", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.RegistryAdd:
                case SysEventCode.RegistrySet:
                case SysEventCode.RegistryDelete:
                    {
                        target = eventlog.TryGetValue("TargetObject", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.RegistryRename:
                    {
                        target = eventlog.TryGetValue("NewName", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.FileCreate:
                case SysEventCode.FileDelete:
                case SysEventCode.FileModified:
                case SysEventCode.CreateStreamHash:
                    {
                        target = eventlog.TryGetValue("TargetFilename", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    break;
                case SysEventCode.Unknown:
                default:
                    // Unknown event code, do nothing
                    return;
            }

            {
                processId = eventlog.TryGetValue("ProcessId", out JToken processIdToken) ? (long)processIdToken : -1;
            }

            if (processId < 0 || string.IsNullOrEmpty(target))
            {
                return;
            }

            this.processActivities.ProcessAction(processId, target, eventCode, eventlog);
        }
    }
}
