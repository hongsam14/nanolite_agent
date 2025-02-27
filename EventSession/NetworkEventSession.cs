namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using global::Nanolite_agent.Beacon;
    using global::Nanolite_agent.Tracepoint;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Tracepoint;

    public sealed class NetworkEventSession : IEventSession
    {
        public readonly string SessionName = "nanolite_network_session";

        private readonly TraceEventSession _traceEventSession;
        private readonly ProcessCreate _processCreate;
        private readonly Beacon _bcon;
        private Task _sessionTask;

        public NetworkEventSession(Beacon bcon)
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
                // Event 3: NetwokTCPIP
                KernelTraceEventParser.Keywords.NetworkTCPIP
                );
        }

        private void registerCallback()
        {
            this._traceEventSession.Source.Kernel.TcpIpConnect += this._bcon.TcpIpConnect;
            this._traceEventSession.Source.Kernel.TcpIpDisconnect += this._bcon.TcpIpDisconnect;
            this._traceEventSession.Source.Kernel.TcpIpSend += this._bcon.TcpIpSend;
            this._traceEventSession.Source.Kernel.TcpIpRecv += this._bcon.TcpIpRecv;
        }
    }
}
