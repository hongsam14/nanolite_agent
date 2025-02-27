// <copyright file="Beacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using Confluent.Kafka;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management;
    using System.Security.Cryptography;
    using Microsoft.CodeAnalysis;
    using Microsoft.Diagnostics.Tracing.AutomatedAnalysis;
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
    using Google.Protobuf;
    using static System.Net.Mime.MediaTypeNames;

    public class Beacon
    {
        public readonly string BeaconName = "nanolite_beacon";

        private TracerProvider _tracerProvider;
        private LoggerProvider _loggerProvider;

        private ActivitySource _spanSource;

        private Dictionary<long, Activity> _processSpan;
        private Dictionary<string, Activity> _tcpIpv4Span;

        private readonly Tracepoint.ProcessCreate _processCreateLog;
        private readonly Tracepoint.ProcessTerminate _processTerminateLog;
        private readonly Tracepoint.NetworkCreate _networkCreate;
        private readonly Tracepoint.NetworkDisconnect _networkDisconnect;

        private bool _isRunning;

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
            this._isRunning = false;
#endif
            // init Otel traceProvider
            try
            {
                this._tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName))
#if DEBUG
                    .AddSource(serviceName)
                    .AddKafkaExporter("localhost", "otel-traces")
                    //.AddOtlpExporter(
                    //options =>
                    //    {
                    //        options.Endpoint = new Uri("http://localhost:4317");
                    //    })
#else
                    .AddKafkaExporter(config.CollectorIP, config.CollectorPort)
#endif
                    .Build();
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }
#if !DEBUG
            // set Source
            if (config.Exporter == null || config.Exporter.Length == 0)
            {
                throw new NanoException.BeaconException(
                    "error while construct beacon.",
                    new NanoException.ConfigException("invalid config. exporter config is null"));
            }
#endif

            // init Otel span source.
            try
            {
#if !DEBUG
                this._spanSource = new ActivitySource(name: config.Exporter);
#else
                this._spanSource = new ActivitySource("TestBed");
#endif

            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            this._processSpan = new Dictionary<long, Activity>();
            this._tcpIpv4Span = new Dictionary<string, Activity>();

            this._processCreateLog = new Tracepoint.ProcessCreate();
            this._processTerminateLog = new Tracepoint.ProcessTerminate();
            this._networkCreate = new Tracepoint.NetworkCreate();
            this._networkDisconnect = new Tracepoint.NetworkDisconnect();

            // set running flag
            this._isRunning = true;
        }

        public void Stop()
        {
            foreach (var activity in this._processSpan)
            {
                activity.Value.Stop();
            }

            foreach (var activity in this._tcpIpv4Span)
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
            Activity pSpan = null;
            Activity span = null;
            ActivityContext spanContext;
            ActivityKind activityKind;
            JObject log = null;
            ulong? gid = null;
            long? pid = null;
            long? ppid = null;
            string image;

            log = this._processCreateLog.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            pid = log.GetValue("ProcessID")?.ToObject<long>();
            ppid = log.GetValue("ParentID")?.ToObject<long>();
            gid = log.GetValue("UniqueProcessKey")?.ToObject<ulong>();
            image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();

            // check parent id
            if (this._processSpan.ContainsKey(ppid.Value))
            {
                // generate sub span
                pSpan = this._processSpan[ppid.Value];
                spanContext = pSpan.Context;
                activityKind = ActivityKind.Internal;
            }
            else
            {
                // there is no context. so create new Root Span context.
                spanContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
                activityKind = ActivityKind.Internal;
            }

            span = this._spanSource.CreateActivity(image, activityKind, spanContext);

            // start pid span
            if (span == null)
            {
                throw new NanoException.BeaconException("Failed to create new span");
            }

            span.SetTag("logsource.product", "windows");
            span.SetTag("process.image", image);


            span.Start();
            span.AddEvent(new ActivityEvent(log.ToString()));

            //Console.WriteLine(log.ToString());

            // For test
            span.SetStatus(ActivityStatusCode.Ok);
            Console.WriteLine(image);

            // add span to processSpan
            this._processSpan[pid.Value] = span;
        }

        public void ProcessTerminate(ProcessTraceData traceData)
        {
            Activity pSpan = null;
            Activity span = null;
            JObject log = null;
            long? pid = null;
            long? ppid = null;
            string image;


            log = this._processTerminateLog.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            pid = log.GetValue("ProcessID")?.ToObject<long>();
            ppid = log.GetValue("ParentID")?.ToObject<long>();
            image = log.GetValue("ImageFileName")?.ToString();

            if (ppid.HasValue && this._processSpan.ContainsKey(ppid.Value))
            {
                // parent pid is exists. activate parent span
                pSpan = this._processSpan[ppid.Value];
            }

            // stop pid span
            if (pid.HasValue && this._processSpan.ContainsKey(pid.Value))
            {
                span = this._processSpan[pid.Value];
                span.AddEvent(new ActivityEvent(log.ToString()));
                //span.SetStatus(ActivityStatusCode.Ok);
                span.Stop();

                this._processSpan.Remove(pid.Value);
            }

            //Console.WriteLine(log.ToString());
        }

        public void TcpIpConnect(TcpIpConnectTraceData traceData)
        {
            Activity pSpan = null;
            Activity span = null;
            ActivityContext spanContext;
            ActivityKind activityKind;
            JObject log = null;
            long? pid = null;
            string image;
            string daddr;
            string saddr;
            string dport;
            string sport;
            string dAddress;
            string sAddress;

            log = this._networkCreate.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            pid = log.GetValue("UsermodePid")?.ToObject<long>();
            image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            daddr = log.GetValue("daddr")?.ToString();
            saddr = log.GetValue("saddr")?.ToString();
            dport = log.GetValue("dport")?.ToString();
            sport = log.GetValue("sport")?.ToString();
            dAddress = $"{daddr}:{dport}";
            sAddress = $"{saddr}:{sport}";

            // check parent id
            if (this._processSpan.ContainsKey(pid.Value))
            {
                // generate sub span
                pSpan = this._processSpan[pid.Value];
                spanContext = pSpan.Context;
                activityKind = ActivityKind.Internal;
            }
            else
            {
                // there is no context. so create new Root Span context.
                spanContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
                activityKind = ActivityKind.Internal;
                pSpan = this._spanSource.CreateActivity(image, activityKind, spanContext);
                pSpan.SetTag("logsource.product", "windows");
                pSpan.SetTag("process.image", image);
                pSpan.SetTag("process.type", "background");

                pSpan.Start();
                pSpan.Stop();

                this._processSpan[pid.Value] = pSpan;
                spanContext = pSpan.Context;
            }

            span = this._spanSource.CreateActivity($"{sAddress}->{dAddress}", activityKind, spanContext);

            // start pid span
            if (span == null)
            {
                throw new NanoException.BeaconException("Failed to create new span");
            }

            span.SetTag("logsource.product", "windows");
            span.SetTag("process.image", image);
            span.SetTag("network.protocol", "TcpIpv4");
            span.SetTag("network.destination", dAddress);
            span.SetTag("network.source", $"{saddr}:{sport}");


            span.Start();
            span.AddEvent(new ActivityEvent(log.ToString()));

            //Console.WriteLine(log.ToString());

            // For test
            span.SetStatus(ActivityStatusCode.Ok);
            Console.WriteLine(image);

            // add span to tcpipv4Span
            this._tcpIpv4Span[$"{sAddress}->{dAddress}"] = span;
        }

        public void TcpIpDisconnect(TcpIpTraceData traceData)
        {
            Activity span = null;
            JObject log = null;
            string daddr;
            string dport;
            string saddr;
            string sport;
            string dAddress;
            string sAddress;


            log = this._networkDisconnect.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            daddr = log.GetValue("daddr")?.ToString();
            saddr = log.GetValue("saddr")?.ToString();
            dport = log.GetValue("dport")?.ToString();
            sport = log.GetValue("sport")?.ToString();
            dAddress = $"{daddr}:{dport}";
            sAddress = $"{saddr}:{sport}";
            
            string key = $"{sAddress}->{dAddress}";

            if (this._tcpIpv4Span.ContainsKey(key))
            {
                // connection is exists. activate parent span
                span = this._tcpIpv4Span[key];

                span.AddEvent(new ActivityEvent(log.ToString()));

                span.Stop();

                this._tcpIpv4Span.Remove(key);
            }

            //Console.WriteLine(log.ToString());
        }

        public void TcpIpSend(TcpIpSendTraceData traceData)
        {
            Activity span = null;
            ActivityContext spanContext;
            ActivityKind activityKind;
            JObject log = null;
            long? pid = null;
            string image;
            string daddr;
            string dport;
            string saddr;
            string sport;
            string dAddress;
            string sAddress;
            string key;

            log = this._processCreateLog.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            pid = log.GetValue("UsermodePid")?.ToObject<long>();
            image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            daddr = log.GetValue("daddr")?.ToString();
            saddr = log.GetValue("saddr")?.ToString();
            dport = log.GetValue("dport")?.ToString();
            sport = log.GetValue("sport")?.ToString();
            dAddress = $"{daddr}:{dport}";
            sAddress = $"{saddr}:{sport}";

            key = $"{sAddress}->{dAddress}";

            // check parent span
            if (this._tcpIpv4Span.ContainsKey(key))
            {
                // append event log to span
                span = this._tcpIpv4Span[key];
                span.AddEvent(new ActivityEvent(log.ToString()));
            }
        }
        
        public void TcpIpRecv(TcpIpTraceData traceData)
        {
            Activity span = null;
            ActivityContext spanContext;
            ActivityKind activityKind;
            JObject log = null;
            long? pid = null;
            string image;
            string daddr;
            string dport;
            string saddr;
            string sport;
            string dAddress;
            string sAddress;
            string key;

            log = this._processCreateLog.EventLog(traceData);
            if (log == null)
            {
                // filtered
                return;
            }

            pid = log.GetValue("UsermodePid")?.ToObject<long>();
            image = this.GetImage(pid.Value) ?? log.GetValue("ImageFileName")?.ToString();
            daddr = log.GetValue("daddr")?.ToString();
            saddr = log.GetValue("saddr")?.ToString();
            dport = log.GetValue("dport")?.ToString();
            sport = log.GetValue("sport")?.ToString();
            dAddress = $"{daddr}:{dport}";
            sAddress = $"{saddr}:{sport}";

            key = $"{sAddress}->{dAddress}";

            // check parent span
            if (this._tcpIpv4Span.ContainsKey(key))
            {
                // append event log to span
                span = this._tcpIpv4Span[key];
                span.AddEvent(new ActivityEvent(log.ToString()));
            }
        }

    }
}
