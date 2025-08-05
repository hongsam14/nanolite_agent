// <copyright file="SystemActivityBeacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

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

        private readonly OtlpLogExporter logExporter;

        // trace provider, processor and span source
        private readonly TracerProvider tracerProvider;

        private readonly ResourceBuilder resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityBeacon"/> class, which sets up tracing and
        /// logging for system activity monitoring using OpenTelemetry.
        /// </summary>
        /// <remarks>This constructor performs the following operations: <list type="bullet">
        /// <item><description>Performs a health check to ensure the OpenTelemetry collector is
        /// reachable.</description></item> <item><description>Initializes the OpenTelemetry tracing provider with the
        /// specified configuration.</description></item> <item><description>Initializes the OpenTelemetry logging
        /// provider with the specified configuration.</description></item> <item><description>Creates a logger and
        /// activity source for system activity monitoring.</description></item> </list> Ensure that the <paramref
        /// name="config"/> object contains valid settings for the collector's IP address, port, and exporter
        /// name.</remarks>
        /// <param name="config">The configuration object containing settings for the beacon, including the collector's IP address, port, and
        /// exporter name. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="NanoException.BeaconException">Thrown if the beacon initialization fails, such as when the collector health check fails or there is an
        /// error constructing the trace and log providers.</exception>
        public SystemActivityBeacon(in Nanolite_agent.Config.Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string serviceName = this.beaconName;
            this.IsRunning = false;

            // init Otel traceProvider
            try
            {
                // health check for the beacon
                try
                {
                    using (HttpClient httpClient1 = new HttpClient())
                    {
                        // check if the collector is reachable
                        var response = httpClient1.GetAsync($"http://{config.CollectorIP}:13133/health").GetAwaiter().GetResult();
                        Console.WriteLine($"Collector health check response: {response.StatusCode}");
                    }
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
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct trace & log providers.", e);
            }

            try
            {
                this.Logger = this.loggerFactory.CreateLogger<Nanolite_agent.Beacon.SystemActivityBeacon>();
                this.SystemActivitySource = new ActivitySource(config.Exporter);
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }
        }

        /// <summary>
        /// Gets the logger for the system activity beacon.
        /// </summary>
        public ILogger<SystemActivityBeacon> Logger { get; private set; }

        /// <summary>
        /// Gets the root activity source for system activities.
        /// </summary>
        public ActivitySource SystemActivitySource { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the beacon is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Starts monitoring the beacon.
        /// </summary>
        /// <remarks>This method sets the beacon to a running state. If the beacon is already running,  an
        /// exception is thrown. Ensure that the beacon is not running before calling this method.</remarks>
        /// <exception cref="NanoException.BeaconException">Thrown if the beacon is already running.</exception>
        public void StartMonitoring()
        {
            if (this.IsRunning)
            {
                throw new NanoException.BeaconException("Beacon is already running.");
            }

            // set running flag
            this.IsRunning = true;
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
            if (!this.IsRunning)
            {
                throw new NanoException.BeaconException("Beacon is not running.");
            }

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
    }
}
