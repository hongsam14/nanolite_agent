// <copyright file="KernelEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Tracepoint;

    /// <summary>
    /// Manages a kernel-mode ETW (Event Tracing for Windows) session to monitor process-related events.
    /// </summary>
    /// <remarks>This class provides functionality to start, stop, and wait for a kernel-mode ETW session. It
    /// listens for  process creation and termination events and uses a <see cref="SystemActivityBeacon"/> to log these
    /// events. The session is configured to automatically stop when disposed and uses a buffer size of 1024
    /// MB.</remarks>
    public class KernelEventSession : IEventSession
    {
        /// <summary>
        /// The name of the ETW (Event Tracing for Windows) session.
        /// </summary>
        /// <remarks>This field holds the default name for the ETW session used in tracing operations. It
        /// is a constant value and cannot be modified.</remarks>
        private readonly string sessionName = "kernel_etw_session";

        private readonly TraceEventSession traceEventSession;
        private readonly KernelProcess kernelProcessTracepoint;
        private readonly SystemActivityBeacon beacon;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="KernelEventSession"/> class,  which manages a kernel event
        /// tracing session and processes system activity events.
        /// </summary>
        /// <remarks>This class sets up a kernel event tracing session using ETW (Event Tracing for
        /// Windows)  and subscribes to relevant kernel process tracepoints. The session is configured to stop
        /// automatically when disposed, with a buffer size of 1024 MB.</remarks>
        /// <param name="bcon">The <see cref="SystemActivityBeacon"/> instance used to signal system activity.  This parameter cannot be
        /// <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bcon"/> is <see langword="null"/>.</exception>
        public KernelEventSession(SystemActivityBeacon bcon)
        {
            // null check for Beacon
            this.beacon = bcon ?? throw new ArgumentNullException(nameof(bcon), "Beacon cannot be null");

            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize Kernel Process Tracepoint
            this.kernelProcessTracepoint = new Tracepoint.KernelProcess();

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

        private void ProcessCreate(ProcessTraceData data)
        {
            if (this.beacon != null)
            {
                try
                {
                    this.beacon.CreateSystemObject(data, this.kernelProcessTracepoint.GetKernelProcessCreateLog);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing kernel event: {ex.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException("Beacon is not initialized. Cannot add log.");
            }
        }

        private void ProcessTerminate(ProcessTraceData data)
        {
            if (this.beacon != null)
            {
                try
                {
                    this.beacon.TerminateSystemObject(data, this.kernelProcessTracepoint.GetKernelProcessStopLog);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing kernel event: {ex.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException("Beacon is not initialized. Cannot add log.");
            }
        }
    }
}
