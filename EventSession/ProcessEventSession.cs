// <copyright file="ProcessEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Tracepoint;

    public sealed class ProcessEventSession : IEventSession
    {
        public readonly string SessionName = "nanolite_process_session";

        private readonly TraceEventSession _traceEventSession;
        private readonly ProcessCreate _processCreate;
        private readonly Beacon _bcon;
        private Task _sessionTask;

        public ProcessEventSession(Beacon bcon)
        {
            this._traceEventSession = new TraceEventSession(SessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024
            };
            this._bcon = bcon;

            this._sessionTask = null;

            this._processCreate = new Tracepoint.ProcessCreate();
            // add privider to etw
            subscribeProvider();
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
                throw new NullReferenceException("sessonTask is not running. Call StartSession before calling WaitSession");
            this._sessionTask.Wait();
        }

        private void subscribeProvider()
        {
            this._traceEventSession.EnableKernelProvider(
                // Event 1: kernel_process_creation
                KernelTraceEventParser.Keywords.Process
                );
            //this._traceEventSession.EnableKernelProvider(new Guid("{22FB2CD6-0E7B-422B-A0C7-2FAD1FD0E716}"),
            //    TraceEventLevel.Always,
            //    (ulong)(0x0000000000000010 | // WINEVENT_KEYWORD_PROCESS
            //    0x0000000000000020 // WINEVENT_KEYWORD_THREAD
            //    ));
        }

        private void registerCallback()
        {
#if DEBUG
            this._traceEventSession.Source.Kernel.ProcessStart += this.debugFunc;
            this._traceEventSession.Source.Kernel.ProcessStop += this.debugFunc;
#else
            this._traceEventSession.Source.Kernel.ProcessStart += this._bcon.ProcessCreate;
            this._traceEventSession.Source.Kernel.ProcessStop += this._bcon.ProcessTerminate;
#endif
            //this._traceEventSession.Source.Kernel.AddCallbackForProviderEvent(
            //    "Microsoft-Windows-Kernel-Process",
            //    "ProcessStart",
            //    this.debugFunc
            //    );
        }

        private void debugFunc(TraceEvent data)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine($"{data.EventName}: {data.ProviderName}");
            for (int i = 0; i < data.PayloadNames.Length; i++)
            {
                Console.WriteLine(data.PayloadNames[i]);
            }
            Console.WriteLine("--------------------");
        }
    }
}
