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
        private readonly SystemActivityBeacon beacon;
        private Task sessionTask;

#if DEBUG
        public SysmonEventSession()
        {
#else
        public SysmonEventSession(SystemActivityBeacon bcon)
        {
            // null check for Beacon
            this.beacon = bcon ?? throw new ArgumentNullException(nameof(bcon), "Beacon cannot be null");
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
            // Start the session and process events in a separate task
            this.sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this.traceEventSession?.Source.Process();
            });
        }

        public void StopSession()
        {
            // Stop the session and the beacon monitoring
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
            //this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processaccessed(rule:ProcessAccess)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processterminated(rule:ProcessTerminate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "ProcessTampering(rule:ProcessTampering)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "CreateRemoteThreaddetected(rule:CreateRemoteThread)", this.ProcessData);

            //this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "RawAccessReaddetected(rule:RawAccessRead)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filecreated(rule:FileCreate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileExecutableDetected(rule:FileExecutableDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filestreamcreated(rule:FileCreateStreamHash)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeleted(rule:FileDelete)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeletelogged(rule:FileDeleteDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filecreationtimechanged(rule:FileCreateTime)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Networkconnectiondetected(rule:NetworkConnect)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Dnsquery(rule:DnsQuery)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Driverloaded(rule:DriverLoad)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Imageloaded(rule:ImageLoad)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryobjectaddedordeleted(rule:RegistryEvent)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryvalueset(rule:RegistryEvent)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryobjectrenamed(rule:RegistryEvent)", this.ProcessData);
        }

        private void ProcessData(TraceEvent data)
        {
            JObject log = this.sysmonTracepoint.GetSysmonLog(data);

            // this means that the log does not pass the filter
            if (log == null)
            {
                return;
            }

#if DEBUG
            // print log
            Console.WriteLine(log.ToString(Newtonsoft.Json.Formatting.None));
#else
            // send log to Beacon
            if (this.bcon != null)
            {
                try
                {
                    this.bcon.SendLog(log);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending log: {e.Message}");
                }
            }
#endif
        }
    }
}