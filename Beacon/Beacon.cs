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

        public Beacon(Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.CollectorIP == null || config.CollectorIP.Length == 0
                || config.CollectorPort == null || config.CollectorPort.Length == 0)
            {
                throw new NanoException.ConfigException("invalid config. collector config is null");
            }

            // init Otel traceProvider
            try
            {
                this._tracerProvider = Sdk.CreateTracerProviderBuilder().AddSource(this.BeaconName)
                    .AddConsoleExporter()
                    .AddOtlpExporter(
                    options =>
                        {
                            options.Endpoint = new Uri($"{config?.CollectorIP}:{config?.CollectorPort}");
                        })
                    .Build();
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            // set Source
            if (config.Exporter == null || config.Exporter.Length == 0)
            {
                throw new NanoException.BeaconException(
                    "error while construct beacon.",
                    new NanoException.ConfigException("invalid config. exporter config is null"));
            }

            // init Otel span source.
            try
            {
                this._spanSource = new ActivitySource(name: config.Exporter);
            }
            catch (Exception e)
            {
                throw new NanoException.BeaconException("error while construct beacon.", e);
            }

            this._processSpan = new Dictionary<long, Activity>();

            this._processCreateLog = new Tracepoint.ProcessCreate();
        }

        public void ProcessCreation(ProcessTraceData traceData)
        {
            Activity pSpan = null;
            Activity span = null;
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

            if (ppid.HasValue && this._processSpan.ContainsKey(ppid.Value))
            {
                // parent pid is exists. activate parent span
                pSpan = this._processSpan[ppid.Value];
                pSpan.Start();
            }

            // start pid span
            span = this._spanSource.StartActivity(image);
            if (span == null)
            {
                throw new NanoException.BeaconException("Failed to create new span");
            }

            span.SetTag("sigma.logsource.category", "process_creation");
            span.SetTag("sigma.logsource.product", "windows");

            this._processSpan[pid.Value] = span;

            span.Start();
            System.Console.WriteLine(log.ToString());
            span.Stop();

            // if pSpan is activated, stop.
            pSpan?.Stop();
        }
    }
}
