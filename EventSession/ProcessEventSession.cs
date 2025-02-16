using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace nanolite_agent.EventSession
{
    public sealed class ProcessEventSession : IEventSession
    {
        public readonly string SessionName = "nanolite_process_session";
        private readonly TraceEventSession traceEventSession;
        private Task sessionTask;
        
        public ProcessEventSession()
        {
            this.traceEventSession = new TraceEventSession(SessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024
            };

            sessionTask = null;
            // add privider to ETW
            subscribeProvider();
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
        }
    }
}
