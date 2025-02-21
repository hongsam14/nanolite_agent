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
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Nanolite_agent.Config;
    using Nanolite_agent.NanoException;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    public class Beacon
    {
        public readonly string BeaconName = "nanolite_beacon";

        private TracerProvider _tracerProvider;
        private LoggerProvider _loggerProvider;

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
            string serviceName = config.Exporter;
#else
            string serviceName = "TestBed";
#endif
            // init Otel traceProvider
            try
            {
                this._tracerProvider = Sdk.CreateTracerProviderBuilder().AddSource(this.BeaconName)
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
#if DEBUG
                    .AddSource(serviceName)
#else
                    .AddSource(serviceName)
#endif
                    //.AddConsoleExporter()
                    .AddOtlpExporter(
                    options =>
                        {
#if DEBUG
                            // Jaeger collector port
                            options.Endpoint = new Uri("http://10.7.0.25:4317");
#else
                            options.Endpoint = new Uri($"{config?.CollectorIP}:{config?.CollectorPort}");
#endif
                        })
                    .Build();

//                var builder = Host.CreateDefaultBuilder().ConfigureLogging(logging =>
//                {
//                    logging.ClearProviders();
//                    logging.AddConsole();

//                    logging.AddOpenTelemetry(options =>
//                    {
//                        options.IncludeFormattedMessage = true;
//                        options.IncludeScopes = true;
//                        options.ParseStateValues = true;

//                        options.AddOtlpExporter(opts =>
//                        {
//#if DEBUG
//                            // Jaeger collector port
//                            opts.Endpoint = new Uri("http://10.7.0.25:4317");
//#else
//                            opts.Endpoint = new Uri($"{config?.CollectorIP}:{config?.CollectorPort}");
//#endif
//                        });
//                    });
//                });

//                var host = builder.Build();

//                var logger = host.Services.GetRequiredService<ILogger<Beacon>>();

//                host.Run();
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

            // beacon start
            Activity span = this._spanSource.StartActivity(SelfInfo.UserName);
            this._processSpan[0] = span;
            span.Start();
        }

        public void Stop()
        {
            foreach (var activity in this._processSpan)
            {
                activity.Value.Stop();
            }
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
                //pSpan = this._spanSource.StartActivity(image, ActivityKind.Internal, default(ActivityContext));
                //if (pSpan == null)
                //{
                //    throw new NanoException.BeaconException("Failed to create new span");
                //}

                //this._processSpan[ppid.Value] = pSpan;
                //pSpan.Start();
                pSpan = this._processSpan[0];
            }

            spanContext = pSpan.Context;
            span = this._spanSource.StartActivity(image, ActivityKind.Internal, spanContext);

            // start pid span
            if (span == null)
            {
                throw new NanoException.BeaconException("Failed to create new span");
            }

            //span.SetTag("sigma.logsource.category", "process_creation");
            span.SetTag("sigma.logsource.product", "windows");

            this._processSpan[pid.Value] = span;

            span.Start();
            span.AddEvent(new ActivityEvent(log.ToString()));

            // For test
            if (image.StartsWith("ms"))
            {
                span.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                span.SetStatus(ActivityStatusCode.Error);
            }
            Console.WriteLine(image);
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
                //span.SetTag("sigma.logsource.category", "process_terminate");
                span.SetTag("sigma.logsource.product", "windows");
                span.AddEvent(new ActivityEvent(log.ToString()));
                //span.SetStatus(ActivityStatusCode.Ok);
                span.Stop();

                this._processSpan.Remove(pid.Value);
            }
        }
    }
}
