using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

namespace nanolite_agent.EventSession
{
    public sealed class ProcessEventSession : IEventSession
    {
        public readonly string SessionName = "nanolite_process_session";
        private readonly TraceEventSession traceEventSession;
        private Task sessionTask;

        private readonly Tracepoint.ProcessCreate processCreate;
        
        public ProcessEventSession()
        {
            this.traceEventSession = new TraceEventSession(SessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024
            };

            sessionTask = null;

            this.processCreate = new Tracepoint.ProcessCreate();
            // add privider to ETW
            subscribeProvider();
            registerCallback();
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
                throw new NullReferenceException("sessonTask is not running. Call StartSession before calling WaitSession");
            this.sessionTask.Wait();
        }

        private void subscribeProvider()
        {
            this.traceEventSession.EnableKernelProvider(
                // Event 1: kernel_process_creation
                KernelTraceEventParser.Keywords.Process
                );
        }

        private void registerCallback()
        {
            this.traceEventSession.Source.Kernel.ProcessStart += this.simpleFunc;
        }

        private void simpleFunc(ProcessTraceData traceData)
        {
            var log = this.processCreate.EventLog(traceData);
            if (log == null)
                return;
        }
    }
}
