// <copyright file="SystemActivityBeacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Beacon.SystemActivity;
    using Nanolite_agent.Config;
    using Nanolite_agent.Helper;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
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
        private readonly BatchLogRecordExportProcessor logProcessor;

        // trace provider, processor and span source
        private readonly TracerProvider tracerProvider;
        private readonly BatchActivityExportProcessor traceProcessor;

        // span source for Otel
        private readonly ActivitySource spanSource;

        // nano-agent config file
        private readonly Nanolite_agent.Config.Config config;

        private readonly ProcessActivitiesOfSystem processActivities;

        // flag to check if beacon is running
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityBeacon"/> class.
        /// </summary>
        /// <param name="config">config.yml file.</param>
        public SystemActivityBeacon(Nanolite_agent.Config.Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string serviceName = config.Exporter;
            this.isRunning = false;

            // init Otel traceProvider
            try
            {
                OtlpExporterOptions option = new OtlpExporterOptions
                {
                    // check config is valid.
                    Endpoint = new Uri($"http://{config.CollectorIP}:{config.CollectorPort}"),
                };

                // initialize traceProvider with OtlpTraceExporter & BatchActivityExportProcessor
                OtlpTraceExporter traceExporter = new OtlpTraceExporter(option);
                this.traceProcessor = new BatchActivityExportProcessor(traceExporter);
                this.tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName))
                    .AddSource(serviceName)
                    .AddProcessor(this.traceProcessor)
                    .Build();

                // initialize log Exporter with OtlpLogExporter & BatchLogExportProcessor
                this.logExporter = new OtlpLogExporter(option);
                this.logProcessor = new BatchLogRecordExportProcessor(this.logExporter);

                this.loggerFactory = LoggerFactory.Create(loggingBuilder =>
                {
                    loggingBuilder.AddOpenTelemetry(options =>
                    {
                        options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                            .AddProcessor(this.logProcessor);
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
                this.spanSource = new ActivitySource(name: config.Exporter);

                // initialize ProcessActivitiesOfSystem
                this.processActivities = new ProcessActivitiesOfSystem(this.logger, this.spanSource);
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            this.config = config;
        }

        public void StartMonitoring()
        {
            if (!this.isRunning)
            {
                throw new NanoException.BeaconException("Beacon is not running.");
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
            this.tracerProvider?.Shutdown();

            // dispose Otel log processor
            this.loggerFactory.Dispose();
        }

        public void SystemActivity(SysEventCode eventCode, JObject eventlog)
        {
            long processId;
            string target;

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    long parentProcessId;
                    {
                        processId = eventlog.TryGetValue("ProcessId", out JToken processIdToken) ? (long)processIdToken : -1;
                        parentProcessId = eventlog.TryGetValue("ParentProcessId", out JToken parentProcessIdToken) ? (long)parentProcessIdToken : -1;
                        target = eventlog.TryGetValue("Image", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    if (processId < 0 || parentProcessId < 0 || string.IsNullOrEmpty(target))
                    {
                        return;
                    }

                    this.processActivities.ProcessLaunch(processId, parentProcessId, target, eventlog);
                    break;
                case SysEventCode.ProcessTerminated:
                    {
                        processId = eventlog.TryGetValue("ProcessId", out JToken processIdToken) ? (long)processIdToken : -1;
                    }

                    if (processId < 0)
                    {
                        return;
                    }

                    this.processActivities.ProcessTerminate(processId, eventlog);
                    break;
                case SysEventCode.Unknown:
                    // Unknown event code, do nothing
                    break;
                default:
                    {
                        processId = eventlog.TryGetValue("ProcessId", out JToken processIdToken) ? (long)processIdToken : -1;
                        target = eventlog.TryGetValue("Target", out JToken targetToken) ? targetToken.ToString() : string.Empty;
                    }

                    if (processId < 0 || string.IsNullOrEmpty(target))
                    {
                        return;
                    }

                    this.processActivities.ProcessAction(processId, target, eventCode, eventlog);
                    break;
            }
        }
    }
}
