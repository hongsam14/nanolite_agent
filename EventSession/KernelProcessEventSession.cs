// <copyright file="KernelProcessEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.NanoException;
    using Nanolite_agent.SystemActivity;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Manages a kernel-mode ETW (Event Tracing for Windows) session to monitor process-related events.
    /// </summary>
    /// <remarks>This class provides functionality to start, stop, and wait for a kernel-mode ETW session. It
    /// listens for  process creation and termination events and uses a <see cref="SystemActivityRecorder"/> to log these
    /// events. The session is configured to automatically stop when disposed and uses a buffer size of 1024
    /// MB.</remarks>
    public class KernelProcessEventSession : IEventSession
    {
        /// <summary>
        /// The name of the ETW (Event Tracing for Windows) session.
        /// </summary>
        /// <remarks>This field holds the default name for the ETW session used in tracing operations. It
        /// is a constant value and cannot be modified.</remarks>
        private readonly string sessionName = "kernel_etw_process_session";

        private readonly TraceEventSession traceEventSession;
        private readonly ETWKernel etwKernelTracepoint;
        private readonly SystemActivityRecorder sysActRecorder;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="KernelProcessEventSession"/> class,  which manages a kernel
        /// process event session for system activity recording.
        /// </summary>
        /// <remarks>This constructor sets up an ETW (Event Tracing for Windows) session with kernel
        /// process tracing enabled.  The session is configured to automatically stop when disposed and uses a buffer
        /// size of 1024 MB.</remarks>
        /// <param name="sysRecorder">The <see cref="SystemActivityRecorder"/> instance used to record system activity.  This parameter cannot be
        /// <see langword="null"/>.</param>
        public KernelProcessEventSession(SystemActivityRecorder sysRecorder)
        {
            // null check for Beacon
            ArgumentNullException.ThrowIfNull(sysRecorder);
            this.sysActRecorder = sysRecorder;

            // initialize etw session
            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize Kernel Process Tracepoint
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
                // blocked until stop is called.
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
                throw new InvalidOperationException("sessonTask is not running. Call StartSession before calling WaitSession");
            }

            this.sessionTask.Wait();
        }

        private void SubscribeProvider()
        {
            this.traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
        }

        private void RegisterCallback()
        {
            this.traceEventSession.Source.Kernel.ProcessStart += this.ProcessCreate;
            this.traceEventSession.Source.Kernel.ProcessStop += this.ProcessTerminate;
        }

        private void ProcessCreate(ProcessTraceData eventData)
        {
            JObject syslog;

            // null check for eventData
            ArgumentNullException.ThrowIfNull(eventData);

            syslog = this.etwKernelTracepoint.GetKernelProcessCreateLog(eventData);
            if (syslog == null)
            {
                // this case means syslog is filtered out by etwKernelTracepoint
                return;
            }

            try
            {
                this.sysActRecorder.StartRecordProcessObject(
                    eventData.ProcessID,
                    eventData.ParentID,
                    eventData.ImageFileName,
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

        private void ProcessTerminate(ProcessTraceData eventData)
        {
            JObject syslog;

            // null check for eventData
            ArgumentNullException.ThrowIfNull(eventData);

            // check the process exists
            if (!this.sysActRecorder.IsProcessTracked(eventData.ProcessID))
            {
                return;
            }

            syslog = this.etwKernelTracepoint.GetKernelProcessCreateLog(eventData);
            if (syslog == null)
            {
                return;
            }

            try
            {
                // stop record process object
                this.sysActRecorder.StopRecordProcessObject(
                    eventData.ProcessID,
                    syslog);
            }
            catch (SystemActivityException ex)
            {
                // log the exception
                Console.WriteLine($"Error in processing process termination: {ex.Message}");
            }
            catch (Exception ex)
            {
                // log the exception
                throw new NanoException.BeaconException(
                    "Error in processing process termination",
                    ex);
            }
        }
    }
}
