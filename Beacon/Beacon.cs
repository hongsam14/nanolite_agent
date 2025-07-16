// <copyright file="Beacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management;
    using Google.Protobuf;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Config;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using static System.Net.Mime.MediaTypeNames;

    public class Beacon
    {
        public readonly string BeaconName = "nanolite_beacon";

        private readonly IHost host;
        private readonly TracerProvider tracerProvider;
        private readonly ActivitySource spanSource;

        private readonly Dictionary<long, Activity> processSpan;
        private readonly Dictionary<string, Activity> tcpIpv4Span;

        //private readonly Tracepoint.NetworkDisconnect networkDisconnect;

        private readonly Config config;
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="Beacon"/> class.
        /// </summary>
        /// <param name="config">config.yml file.</param>
        public Beacon(Nanolite_agent.Config.Config config)
        {
#if !DEBUG
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.CollectorIP == null || config.CollectorIP.Length == 0
                || config.CollectorPort == null || config.CollectorPort.Length == 0)
            {
                throw new NanoException.ConfigException("invalid config. collector config is null");
            }
            string serviceName = config.Exporter;
#else
            string serviceName = "TestBed";
            this.isRunning = false;
#endif

            // init Otel traceProvider
            try
            {
                OtlpExporterOptions option = new OtlpExporterOptions
                {
#if DEBUG
                    Endpoint = new Uri($"http://localhost:4317"),
#else
                    // check config is valid.
                    Endpoint = new Uri($"http://{config.CollectorIP}:{config.CollectorPort}"),
                    if (config.Exporter == null || config.Exporter.Length == 0)
                    {
                        throw new NanoException.BeaconException(
                            "error while construct beacon.",
                            new NanoException.ConfigException("invalid config. exporter config is null"));
                    }
#endif
                };

                // initialize traceProvider with OtlpTraceExporter & BatchActivityExportProcessor
                OtlpTraceExporter traceExporter = new OtlpTraceExporter(option);
                BatchActivityExportProcessor traceProcessor = new BatchActivityExportProcessor(traceExporter);
                this.tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName))
                    .AddSource(serviceName)
                    .AddProcessor(traceProcessor)
                    .Build();

                // initialize log Exporter with OtlpLogExporter & BatchLogExportProcessor
                OtlpLogExporter logExporter = new OtlpLogExporter(option);
                BatchLogRecordExportProcessor logProcessor = new BatchLogRecordExportProcessor(logExporter);
                // TODO: Add custom logger -> OpenTelemetry.Logs
                this.host = Host.CreateDefaultBuilder()
                    .ConfigureServices((_, services) =>
                    {
                        services.AddLogging(loggingBuilder =>
                        {
                            loggingBuilder.AddOpenTelemetry(options =>
                            {
                                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                                    .AddProcessor(logProcessor);
                            });
                        });
                    }).Build();
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            // init Otel span source.
            try
            {
#if !DEBUG
                this._spanSource = new ActivitySource(name: config.Exporter);
#else
                this.spanSource = new ActivitySource("TestBed");
#endif
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            this.processSpan = new Dictionary<long, Activity>();
            this.tcpIpv4Span = new Dictionary<string, Activity>();

            //this.processCreateLog = new Tracepoint.ProcessCreate();
            //this.processTerminateLog = new Tracepoint.ProcessTerminate();
            //this.networkCreate = new Tracepoint.NetworkCreate();
            //this.networkDisconnect = new Tracepoint.NetworkDisconnect();

            // set running flag
            this.isRunning = true;
            this.config = config;
        }

        public void Stop()
        {
            foreach (var activity in this.processSpan)
            {
                activity.Value.Stop();
            }

            foreach (var activity in this.tcpIpv4Span)
            {
                activity.Value.Stop();
            }
        }

        private string GetImage(long processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["ExecutablePath"]?.ToString() ?? null;
                    }
                }
            }
            catch { }
            return null;
        }

        public void ProcessCreation(ProcessTraceData traceData)
        {
            //Activity pSpan = null;
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

        public void ProcessTerminate(ProcessTraceData traceData)
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
