// <copyright file="KernelEventSession.cs" company="PlaceholderCompany">
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
    using Nanolite_agent.Helper;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    public class KernelEventSession : IEventSession
    {
        public readonly string sessionName = "kernel_etw_session";
        public readonly string providerProcessGUID = "22fb2cd6-0e7b-422b-a0c7-2fad1fd0e716";

        private readonly TraceEventSession traceEventSession;
        private readonly KernelProcess kernelProcessTracepoint;
        private readonly SystemActivityBeacon beacon;
        private Task sessionTask;

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
            {
                throw new NullReferenceException("sessonTask is not running. Call StartSession before calling WaitSession");
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
            JToken metadataToken;
            JObject log = this.kernelProcessTracepoint.GetKernelProcessCreateLog(data);

            // this means that the log does not pass the filter
            if (log == null)
            {
                return;
            }

            if (this.beacon != null)
            {
                try
                {
                    // get metadata from log
                    if (log.TryGetValue("Metadata", out metadataToken))
                    {
                        // convert jtoken to JObject
                        this.beacon.CreateSystemObject(metadataToken.ToObject<JObject>());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing kernel event: {ex.Message}");
                }
            }
            else
            {
                throw new NullReferenceException("Beacon is not initialized. Cannot add log.");
            }
        }

        private void ProcessTerminate(ProcessTraceData data)
        {
            JToken metadataToken;
            JObject log = this.kernelProcessTracepoint.GetKernelProcessStopLog(data);

            // this means that the log does not pass the filter
            if (log == null)
            {
                return;
            }

            if (this.beacon != null)
            {
                try
                {
                    // get metadata from log
                    if (log.TryGetValue("Metadata", out metadataToken))
                    {
                        // convert jtoken to JObject
                        this.beacon.TerminateSystemObject(metadataToken.ToObject<JObject>());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing kernel event: {ex.Message}");
                }
            }
            else
            {
                throw new NullReferenceException("Beacon is not initialized. Cannot add log.");
            }
        }
    }
}
