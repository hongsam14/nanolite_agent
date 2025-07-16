// <copyright file="ProcessEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    public sealed class SysmonEventSession : IEventSession
    {
        public readonly string SessionName = "Microsoft-Windows-Sysmon";
        public readonly string providerGUID = "5770385f-c22a-43e0-bf4c-06f5698ffbd9";

        private readonly TraceEventSession _traceEventSession;
        private readonly Sysmon _sysmonTracepoint;
        private readonly Beacon _bcon;
        private Task _sessionTask;

        public SysmonEventSession()
        {
            this._traceEventSession = new TraceEventSession(SessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024
            };

            this._sessionTask = null;

            this._sysmonTracepoint = new Tracepoint.Sysmon();
            // add privider to etw
            SubscribeProvider();
            registerCallback();
        }

        public void StartSession()
        {
            this._sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this._traceEventSession?.Source.Process();
            });
        }

        public void StopSession()
        {
            this._traceEventSession?.Stop();
        }

        public void WaitSession()
        {
            if (this._sessionTask == null)
            {
                throw new NullReferenceException("sessonTask is not running. Call StartSession before calling WaitSession");
            }

            this._sessionTask.Wait();
        }

        private void SubscribeProvider()
        {
            this._traceEventSession.EnableProvider(this.providerGUID);
        }

        private void registerCallback()
        {
#if DEBUG
            //this._traceEventSession.Source.Dynamic.All += this.debugFunc;
            this._traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.SessionName, "ProcessCreate(rule:ProcessCreate)", this.debugFunc);
#else
            this._traceEventSession.Source.Kernel.ProcessStart += this._bcon.ProcessCreate;
            this._traceEventSession.Source.Kernel.ProcessStop += this._bcon.ProcessTerminate;
#endif
        }

        private void debugFunc(TraceEvent data)
        {
            JObject log = this._sysmonTracepoint.GetSysmonLog(data);
            if (log == null)
            {
                return;
            }

            // print log
            Console.WriteLine(log.ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}