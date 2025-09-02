// <copyright file="KernelThreadEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.NanoException;
    using Nanolite_agent.SystemActivity;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;


    /// <summary>
    /// Manages an Event Tracing for Windows (ETW) session for kernel thread events, enabling asynchronous monitoring
    /// and recording of system process/thread activity. This class sets up a dedicated ETW session to capture kernel
    /// thread start events, applies filtering and decoding via <see cref="ETWKernel"/>, and records relevant activity
    /// using <see cref="SystemActivityRecorder"/>. It provides methods to start, stop, and wait for the session,
    /// ensuring proper resource management and error handling for system activity logging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Usage:</b> Instantiate with a <see cref="SystemActivityRecorder"/> and call <see cref="StartSession"/> to begin
    /// monitoring. Use <see cref="StopSession"/> to halt the session and <see cref="WaitSession"/> to block until completion.
    /// </para>
    /// <para>
    /// <b>Responsibilities:</b>
    /// <list type="bullet">
    /// <item>Initializes and configures a kernel ETW session for thread events.</item>
    /// <item>Subscribes to kernel thread start events and processes them asynchronously.</item>
    /// <item>Filters, decodes, and records thread start events, associating them with process activity contexts.</item>
    /// <item>Handles duplicate event prevention and error propagation for robust monitoring.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class KernelThreadEventSession : IEventSession
    {
        /// <summary>
        /// The name of the ETW (Event Tracing for Windows) session.
        /// </summary>
        /// <remarks>This field holds the default name fore the ETW session used in tracing operations.
        /// It is a constant value and cannot be modified.</remarks>
        private readonly string sessionName = "kernel_etw_thread_session";

        private readonly TraceEventSession traceEventSession;
        private readonly ETWKernel etwKernelTracepoint;
        private readonly SystemActivityRecorder sysActRecorder;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="KernelThreadEventSession"/> class, which manages a kernel
        /// process event session for system activity recording.
        /// </summary>
        /// <remarks>This constructor sets up an ETW (Event Tracing for Windows) session with kernel
        /// thread tracing enabled. The session is configured to automatically stop when disposed and uses a buffer
        /// size of 1024 MB. It also registers callbacks for thread start and stop events.</remarks>
        /// <param name="sysRecorder">The <see cref="SystemActivityRecorder"/> instance used to record system activity.
        /// This parameter cannot be <see langword="null"/>.</param>
        public KernelThreadEventSession(SystemActivityRecorder sysRecorder)
        {
            // null check for recorder
            ArgumentNullException.ThrowIfNull(sysRecorder);
            this.sysActRecorder = sysRecorder;

            // initialize ETW session
            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize kernel process tracepoint
            this.etwKernelTracepoint = new Tracepoint.ETWKernel();

            // subscribe function to etw session
            this.SubscribeProvider();
            this.RegisterCallback();
        }

        /// <summary>
        /// Starts a new session for processing trace events asynchronously.
        /// </summary>
        /// <remarks>This method begins processing trace events on a background task.  The session will
        /// continue running until explicitly stopped. Ensure that the session is properly stopped to release
        /// resources.</remarks>
        public void StartSession()
        {
            this.sessionTask = Task.Run(() =>
            {
                this.traceEventSession?.Source.Process();
            });
        }

        /// <summary>
        /// Stops the current trace event session, if one is active.
        /// </summary>
        /// <remarks>This method halts the trace event session associated with the instance.  If no
        /// session is active, the method performs no action.</remarks>
        public void StopSession()
        {
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
                throw new InvalidOperationException("Session task is not running. Call StartSession before calling WaitSession.");
            }

            this.sessionTask.Wait();
        }

        private void SubscribeProvider()
        {
            // subscribe to kernel thread events
            this.traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.Thread);
        }

        private void RegisterCallback()
        {
            // register callback for thread events
            this.traceEventSession.Source.Kernel.ThreadStart += this.ThreadStart;
        }

        private void ThreadStart(ThreadTraceData eventData)
        {
            JObject syslog;

            // null check for eventData
            ArgumentNullException.ThrowIfNull(eventData);

            // if pid is exists in map, skip this event to prevent duplicate processing
            if (this.sysActRecorder.IsProcessTracked(eventData.ProcessID))
            {
                return;
            }

            syslog = this.etwKernelTracepoint.GetKernelThreadStartLog(eventData);
            if (syslog == null)
            {
                // this case means syslog is filtered out by etwKernelTracepoint
                return;
            }

            // get image name with process id.
            string imageName = syslog["ImageFileName"]?.ToString() ?? "unknown";

            try
            {
                this.sysActRecorder.StartRecordProcessObject(
                    eventData.ProcessID,
                    eventData.ParentProcessID,
                    imageName,
                    syslog);
            }
            catch (SystemActivityException ex)
            {
                // log the exception
                Console.WriteLine($"Error in processing process creation: {ex.Message}");
            }
            catch (Exception ex)
            {
                // log the exception
                throw new NanoException.BeaconException(
                    "Error in processing process creation",
                    ex);
            }
        }
    }
}
