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
    using Nanolite_agent.Helper;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a session for monitoring system activity using Sysmon events.
    /// </summary>
    /// <remarks>This class provides functionality to manage a trace event session for capturing and
    /// processing Sysmon events. It allows starting, stopping, and waiting for the session, as well as handling event
    /// callbacks for various Sysmon event types. The session is automatically configured to stop when
    /// disposed.</remarks>
    public sealed class SysmonEventSession : IEventSession
    {
        /// <summary>
        /// Gets the name of the session used for monitoring system events.
        /// </summary>
        private readonly string sessionName = "Microsoft-Windows-Sysmon";

        /// <summary>
        /// Gets the unique identifier for the provider.
        /// </summary>
        private readonly string providerGUID = "5770385f-c22a-43e0-bf4c-06f5698ffbd9";

        private readonly TraceEventSession traceEventSession;
        private readonly Sysmon sysmonTracepoint;
        private readonly SystemActivityBeacon beacon;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="SysmonEventSession"/> class,  which manages a session for
        /// monitoring system activity using Sysmon events.
        /// </summary>
        /// <remarks>This constructor sets up the necessary components for monitoring Sysmon events,
        /// including initializing a trace event session and configuring a Sysmon tracepoint.  The session is
        /// automatically configured to stop when disposed.</remarks>
        /// <param name="bcon">The <see cref="SystemActivityBeacon"/> instance used to coordinate system activity monitoring.  This
        /// parameter cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bcon"/> is <see langword="null"/>.</exception>
        public SysmonEventSession(SystemActivityBeacon bcon)
        {
            // null check for Beacon
            this.beacon = bcon ?? throw new ArgumentNullException(nameof(bcon), "Beacon cannot be null");

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

        /// <summary>
        /// Starts a new session and begins processing events asynchronously.
        /// </summary>
        /// <remarks>This method initiates the session and processes events in a background task.  The
        /// session will continue running until explicitly stopped.</remarks>
        public void StartSession()
        {
            // Start the session and process events in a separate task
            this.sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this.traceEventSession?.Source.Process();
            });
        }

        /// <summary>
        /// Stops the current trace event session and halts beacon monitoring.
        /// </summary>
        /// <remarks>This method terminates the active trace event session if one is running.  It is safe
        /// to call this method multiple times; subsequent calls will have no effect  if the session has already been
        /// stopped.</remarks>
        public void StopSession()
        {
            // Stop the session and the beacon monitoring
            this.traceEventSession?.Stop();
        }

        /// <summary>
        /// Waits for the current session task to complete.
        /// </summary>
        /// <remarks>This method blocks the calling thread until the session task, initiated by <see
        /// cref="StartSession"/>, completes. Ensure that <see cref="StartSession"/> has been called before invoking
        /// this method.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session task is not running. Call <see cref="StartSession"/> before calling this method.</exception>
        public void WaitSession()
        {
            if (this.sessionTask == null)
            {
                throw new InvalidOperationException("sessonTask is not running. Call StartSession before calling WaitSession");
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
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "ProcessTampering(rule:ProcessTampering)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "CreateRemoteThreaddetected(rule:CreateRemoteThread)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "RawAccessReaddetected(rule:RawAccessRead)", this.ProcessData);

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
            SysEventCode code = SysmonEventDecoder.GetEventCodeFromData(data);

            // send log to Beacon
            if (this.beacon != null)
            {
                try
                {
                    this.beacon.ConsumeSystemActivity(code, data, this.sysmonTracepoint.GetSysmonLog);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending log: {e.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException("Beacon is not initialized. Cannot add log.");
            }
        }
    }
}