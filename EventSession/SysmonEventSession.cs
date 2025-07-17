// <copyright file="SysmonEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    public sealed class SysmonEventSession : IEventSession
    {
        private readonly string sessionName = "Microsoft-Windows-Sysmon";
        private readonly string providerGUID = "5770385f-c22a-43e0-bf4c-06f5698ffbd9";

        private readonly TraceEventSession traceEventSession;
        private readonly Sysmon sysmonTracepoint;
        private readonly Beacon bcon;
        private Task sessionTask;

#if DEBUG
        public SysmonEventSession()
        {
#else
        public SysmonEventSession(Beacon bcon)
        {
            // null check for Beacon
            this.bcon = bcon ?? throw new ArgumentNullException(nameof(bcon), "Beacon cannot be null");
#endif

            // initialize TraceEventSession
            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize Sysmon Tracepoint
            this.sysmonTracepoint = new Tracepoint.Sysmon();

            // subscribe function to etw session
            this.SubscribeProvider();
            this.RegisterCallback();
        }

        public void StartSession()
        {
            this.sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this.traceEventSession?.Source.Process();
            });
        }

        public void StopSession()
        {
            this.traceEventSession?.Stop();
        }

        public void WaitSession()
        {
            if (this.sessionTask == null)
            {
                throw new NullReferenceException("sessonTask is not running. Call StartSession before calling WaitSession");
            }

            this.sessionTask.Wait();
        }

        private void SubscribeProvider()
        {
            this.traceEventSession.EnableProvider(this.providerGUID);
        }

        private void RegisterCallback()
        {
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "ProcessCreate(rule:ProcessCreate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processaccessed(rule:ProcessAccess)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processterminated(rule:ProcessTerminate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filecreated(rule:FileCreate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileExecutableDetected(rule:FileExecutableDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filestreamcreated(rule:FileCreateStreamHash)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeleted(rule:FileDelete)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeletelogged(rule:FileDeleteDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Networkconnectiondetected(rule:NetworkConnect)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Dnsquery(rule:DnsQuery)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Driverloaded(rule:DriverLoad)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Imageloaded(rule:ImageLoad)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryobjectaddedordeleted(rule:RegistryEvent)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryvalueset(rule:RegistryEvent)", this.ProcessData);
        }

        private void ProcessData(TraceEvent data)
        {
            JObject log = this.sysmonTracepoint.GetSysmonLog(data);
            if (log == null)
            {
                return;
            }
#if DEBUG
            // print log
            Console.WriteLine(log.ToString(Newtonsoft.Json.Formatting.None));
#endif
        }
    }
}