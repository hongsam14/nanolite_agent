// <copyright file="Beacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
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

            // init Otel span source.
            try
            {
                this.spanSource = new ActivitySource(name: config.Exporter);
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

            // flush and shutdown Otel log exporter
            this.logExporter?.ForceFlush();
            this.logExporter?.Shutdown();

            // flush and shutdown Otel trace provider
            this.tracerProvider?.ForceFlush();
            this.tracerProvider?.Shutdown();

            // dispose Otel log processor
            this.loggerFactory.Dispose();

        }

        public void ProcessActivity(SysEventCode eventCode, JObject eventlog)
        {
            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    this.ProcessLaunch(eventlog);
                    break;
                default:
                    // Unknown event code, do nothing
                    break;
            }
        }

        public void FileSystemActivity(SysEventCode eventCode, JObject eventlog)
        {
        }

        public void NetworkActivity(SysEventCode eventCode, JObject eventlog)
        {
        }

        private void ProcessLaunch(JObject traceData)
        {
            //Activity span = null;
            //ActivityContext spanContext;
            //ActivityKind activityKind;
            //JObject log = null;
            //ulong? gid = null;
            //long? pid = null;
            //long? ppid = null;
            //string image;

            //log = this.processCreateLog.EventLog(traceData);
            //if (log == null)
            //{
            //    // filtered
            //    return;
            //}

            //pid = log.GetValue("ProcessID")?.ToObject<long>();
            //ppid = log.GetValue("ParentID")?.ToObject<long>();
            //gid = log.GetValue("UniqueProcessKey")?.ToObject<ulong>();
            //image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();

            //// check parent id
            //if (this.processSpan.ContainsKey(ppid.Value))
            //{
            //    // generate sub span
            //    pSpan = this.processSpan[ppid.Value];
            //    spanContext = pSpan.Context;
            //    activityKind = ActivityKind.Internal;
            //}
            //else
            //{
            //    // there is no context. so create new Root Span context.
            //    spanContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
            //    activityKind = ActivityKind.Internal;
            //}

            //span = this.spanSource.CreateActivity(image, activityKind, spanContext);

            //// start pid span
            //if (span == null)
            //{
            //    throw new NanoException.BeaconException("Failed to create new span");
            //}

            //span.SetTag("logsource.product", "windows");
            //span.SetTag("process.image", image);


            //span.Start();
            //span.AddEvent(new ActivityEvent(log.ToString()));

            //Console.WriteLine(log.ToString());

            //// For test
            //span.SetStatus(ActivityStatusCode.Ok);

            //// add span to processSpan
            //this.processSpan[pid.Value] = span;
        }

        public void ProcessTerminate(JObject traceData)
        {
            //Activity pSpan = null;
            //Activity span = null;
            //JObject log = null;
            //long? pid = null;
            //long? ppid = null;
            //string image;


            //log = this.processTerminateLog.EventLog(traceData);
            //if (log == null)
            //{
            //    // filtered
            //    return;
            //}

            //pid = log.GetValue("ProcessID")?.ToObject<long>();
            //ppid = log.GetValue("ParentID")?.ToObject<long>();
            //image = log.GetValue("ImageFileName")?.ToString();

            //if (ppid.HasValue && this.processSpan.ContainsKey(ppid.Value))
            //{
            //    // parent pid is exists. activate parent span
            //    pSpan = this.processSpan[ppid.Value];
            //}

            //// stop pid span
            //if (pid.HasValue && this.processSpan.ContainsKey(pid.Value))
            //{
            //    span = this.processSpan[pid.Value];
            //    span.AddEvent(new ActivityEvent(log.ToString()));
            //    //span.SetStatus(ActivityStatusCode.Ok);
            //    span.Stop();

            //    this.processSpan.Remove(pid.Value);
            //}

            //Console.WriteLine(log.ToString());
        }

        public void TcpIpConnect(TcpIpConnectTraceData traceData)
        {
            //Activity pSpan = null;
            //Activity span = null;
            //ActivityContext spanContext;
            //ActivityKind activityKind;
            //JObject log = null;
            //long? pid = null;
            //string image;
            //string daddr;
            //string saddr;
            //string dport;
            //string sport;
            //string dAddress;
            //string sAddress;

            //log = this.networkCreate.EventLog(traceData);
            //if (log == null)
            //{
            //    // filtered
            //    return;
            //}

            //pid = log.GetValue("UsermodePid")?.ToObject<long>();
            //image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            //daddr = log.GetValue("daddr")?.ToString();
            //saddr = log.GetValue("saddr")?.ToString();
            //dport = log.GetValue("dport")?.ToString();
            //sport = log.GetValue("sport")?.ToString();
            //dAddress = $"{daddr}:{dport}";
            //sAddress = $"{saddr}:{sport}";

            //// check parent id
            //if (this.processSpan.ContainsKey(pid.Value))
            //{
            //    // generate sub span
            //    pSpan = this.processSpan[pid.Value];
            //    spanContext = pSpan.Context;
            //    activityKind = ActivityKind.Internal;
            //}
            //else
            //{
            //    // there is no context. so create new Root Span context.
            //    spanContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
            //    activityKind = ActivityKind.Internal;
            //    pSpan = this.spanSource.CreateActivity(image, activityKind, spanContext);
            //    pSpan.SetTag("logsource.product", "windows");
            //    pSpan.SetTag("process.image", image);
            //    pSpan.SetTag("process.type", "background");

            //    pSpan.Start();
            //    pSpan.Stop();

            //    this.processSpan[pid.Value] = pSpan;
            //    spanContext = pSpan.Context;
            //}

            //span = this.spanSource.CreateActivity($"{sAddress}->{dAddress}", activityKind, spanContext);

            //// start pid span
            //if (span == null)
            //{
            //    throw new NanoException.BeaconException("Failed to create new span");
            //}

            //span.SetTag("logsource.product", "windows");
            //span.SetTag("process.image", image);
            //span.SetTag("network.protocol", "TcpIpv4");
            //span.SetTag("network.destination", dAddress);
            //span.SetTag("network.source", $"{saddr}:{sport}");


            //span.Start();
            //span.AddEvent(new ActivityEvent(log.ToString()));

            //Console.WriteLine(log.ToString());

            //// For test
            //span.SetStatus(ActivityStatusCode.Ok);

            //// add span to tcpipv4Span
            //this.tcpIpv4Span[$"{sAddress}->{dAddress}"] = span;
        }

        public void TcpIpDisconnect(TcpIpTraceData traceData)
        {
            //Activity span = null;
            //JObject log = null;
            //string daddr;
            //string dport;
            //string saddr;
            //string sport;
            //string dAddress;
            //string sAddress;


            //log = this.networkDisconnect.EventLog(traceData);
            //if (log == null)
            //{
            //    // filtered
            //    return;
            //}

            //daddr = log.GetValue("daddr")?.ToString();
            //saddr = log.GetValue("saddr")?.ToString();
            //dport = log.GetValue("dport")?.ToString();
            //sport = log.GetValue("sport")?.ToString();
            //dAddress = $"{daddr}:{dport}";
            //sAddress = $"{saddr}:{sport}";

            //string key = $"{sAddress}->{dAddress}";

            //if (this.tcpIpv4Span.ContainsKey(key))
            //{
            //    // connection is exists. activate parent span
            //    span = this.tcpIpv4Span[key];

            //    span.AddEvent(new ActivityEvent(log.ToString()));

            //    span.Stop();

            //    this.tcpIpv4Span.Remove(key);
            //}

            //Console.WriteLine(log.ToString());
        }

        public void TcpIpSend(TcpIpSendTraceData traceData)
        {
            //Activity span = null;
            //ActivityContext spanContext;
            //ActivityKind activityKind;
            //JObject log = null;
            //long? pid = null;
            //string image;
            //string daddr;
            //string dport;
            //string saddr;
            //string sport;
            //string dAddress;
            //string sAddress;
            //string key;

            //log = this.processCreateLog.EventLog(traceData);
            //if (log == null)
            //{
            //    // filtered
            //    return;
            //}

            //pid = log.GetValue("UsermodePid")?.ToObject<long>();
            //image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            //daddr = log.GetValue("daddr")?.ToString();
            //saddr = log.GetValue("saddr")?.ToString();
            //dport = log.GetValue("dport")?.ToString();
            //sport = log.GetValue("sport")?.ToString();
            //dAddress = $"{daddr}:{dport}";
            //sAddress = $"{saddr}:{sport}";

            //key = $"{sAddress}->{dAddress}";

            //// check parent span
            //if (this.tcpIpv4Span.ContainsKey(key))
            //{
            //    // append event log to span
            //    span = this.tcpIpv4Span[key];
            //    span.AddEvent(new ActivityEvent(log.ToString()));
            //}
        }

        public void TcpIpRecv(TcpIpTraceData traceData)
        {
            //    Activity span = null;
            //    ActivityContext spanContext;
            //    ActivityKind activityKind;
            //    JObject log = null;
            //    long? pid = null;
            //    string image;
            //    string daddr;
            //    string dport;
            //    string saddr;
            //    string sport;
            //    string dAddress;
            //    string sAddress;
            //    string key;

            //    log = this.processCreateLog.EventLog(traceData);
            //    if (log == null)
            //    {
            //        // filtered
            //        return;
            //    }

            //    pid = log.GetValue("UsermodePid")?.ToObject<long>();
            //    image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            //    daddr = log.GetValue("daddr")?.ToString();
            //    saddr = log.GetValue("saddr")?.ToString();
            //    dport = log.GetValue("dport")?.ToString();
            //    sport = log.GetValue("sport")?.ToString();
            //    dAddress = $"{daddr}:{dport}";
            //    sAddress = $"{saddr}:{sport}";

            //    key = $"{sAddress}->{dAddress}";

            //    // check parent span
            //    if (this.tcpIpv4Span.ContainsKey(key))
            //    {
            //        // append event log to span
            //        span = this.tcpIpv4Span[key];
            //        span.AddEvent(new ActivityEvent(log.ToString()));
            //    }
        }

    }
}
