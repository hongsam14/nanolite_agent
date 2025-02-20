// <copyright file="Beacon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Nanolite_agent.Config;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    public class Beacon
    {
        public readonly string BeaconName = "nanolite_beacon";

        private TracerProvider _tracerProvider;

        private ActivitySource _spanSource;

        private Dictionary<long, Activity> _processSpan;

        private readonly Tracepoint.ProcessCreate _processCreateLog;
        private readonly Tracepoint.ProcessTerminate _processTerminateLog;

        public Beacon(Config config)
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
#endif
            // init Otel traceProvider
            try
            {
                this._tracerProvider = Sdk.CreateTracerProviderBuilder().AddSource(this.BeaconName)
                    .AddConsoleExporter()
                    .AddOtlpExporter(
                    options =>
                        {
#if DEBUG
                            // Jaeger collector port
                            options.Endpoint = new Uri("localhost:14250");
#else
                            options.Endpoint = new Uri($"{config?.CollectorIP}:{config?.CollectorPort}");
#endif
                        })
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

            this._processCreateLog = new Tracepoint.ProcessCreate();
            this._processTerminateLog = new Tracepoint.ProcessTerminate();
        }

        public void ProcessCreation(ProcessTraceData traceData)
        {
            Activity pSpan = null;
            Activity span = null;
            ActivityContext spanContext;
            JObject log = null;
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
            image = log.GetValue("ImageFileName")?.ToString();

            if (this._processSpan.ContainsKey(ppid.Value))
            {
                pSpan = this._processSpan[ppid.Value];
            }
            else
            {
                // when parent pid exists. activate parent span because it's already exists before agent start
                pSpan = this._spanSource.StartActivity(image);
                if (pSpan == null)
                {
                    throw new NanoException.BeaconException("Failed to create new span");
                }

                pSpan.Start();
            }

            spanContext = pSpan.Context;

            // start pid span
            span = this._spanSource.StartActivity(image, ActivityKind.Internal, spanContext);
            if (span == null)
            {
                throw new NanoException.BeaconException("Failed to create new span");
            }

            span.SetTag("sigma.logsource.category", "process_creation");
            span.SetTag("sigma.logsource.product", "windows");

            this._processSpan[pid.Value] = span;

            span.Start();
            span.AddEvent(new ActivityEvent(log.ToString()));
            //span.SetStatus(ActivityStatusCode.Ok);
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
                //pSpan.Stop();
            }

            // stop pid span
            if (pid.HasValue && this._processSpan.ContainsKey(pid.Value))
            {
                span = this._processSpan[pid.Value];
                span.SetTag("sigma.logsource.category", "process_terminate");
                span.SetTag("sigma.logsource.product", "windows");
                span.AddEvent(new ActivityEvent(log.ToString()));
                //span.SetStatus(ActivityStatusCode.Ok);
                span.Stop();
            }
        }
    }
}
