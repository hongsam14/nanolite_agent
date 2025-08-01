// <copyright file="KernelProcess.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a process that handles kernel events and provides their log representation.
    /// </summary>
    /// <remarks>The <see cref="ETWKernel"/> class is designed to process kernel events by applying pre
    /// and post filter functions. If an event passes both filters, it is decoded into a JSON object for logging
    /// purposes.</remarks>
    public class ETWKernel : BaseTracepoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ETWKernel"/> class with a default process name.
        /// </summary>
        public ETWKernel()
            : base("kernel_process")
        {
        }

        /// <summary>
        /// Processes a kernel process creation event and returns a structured log representation.
        /// </summary>
        /// <remarks>This method applies pre- and post-filtering functions to determine if the event
        /// should be processed. If the event passes both filters, it is decoded into a JSON object.</remarks>
        /// <param name="data">The <see cref="ProcessTraceData"/> containing the event data to be processed. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded kernel process creation event, or <see langword="null"/> if
        /// the event is filtered out.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the event data cannot be decoded into a valid log entry.</exception>
        public JObject GetKernelProcessCreateLog(ProcessTraceData data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // run pre filter function
            if (!this.PreFilterFunc(data))
            {
                return null;
            }

            log = KernelEventDecoder.DecodeKernelProcessCreateEvent(data);
            if (log == null)
            {
                throw new ArgumentException(DebugMessages.TracepointAcceptErrMessage);
            }

            // run post filter function
            if (!this.PostFilterFunc(log))
            {
                return null;
            }

            return log;
        }

        /// <summary>
        /// Retrieves a log entry for a kernel process stop event.
        /// </summary>
        /// <param name="data">The trace data associated with the kernel process stop event. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded log entry for the kernel process stop event, or <see
        /// langword="null"/> if the event does not pass the pre or post filter functions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the trace data cannot be decoded into a valid log entry.</exception>
        public JObject GetKernelProcessStopLog(ProcessTraceData data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // run pre filter function
            if (!this.PreFilterFunc(data))
            {
                return null;
            }

            log = KernelEventDecoder.DecodeKernelProcessStopEvent(data);
            if (log == null)
            {
                throw new ArgumentException(DebugMessages.TracepointAcceptErrMessage);
            }

            // run post filter function
            if (!this.PostFilterFunc(log))
            {
                return null;
            }

            return log;
        }

        /// <summary>
        /// Decodes a kernel registry query event from the provided trace data and applies pre- and post-filtering
        /// logic.
        /// </summary>
        /// <remarks>This method first applies a pre-filtering function to determine whether the trace
        /// data should be processed.  If the pre-filtering function returns <see langword="false"/>, the method returns
        /// <see langword="null"/>. After decoding the trace data, a post-filtering function is applied to the resulting
        /// <see cref="JObject"/>.  If the post-filtering function returns <see langword="false"/>, the method also
        /// returns <see langword="null"/>.</remarks>
        /// <param name="data">The trace data containing information about the kernel registry query event. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded kernel registry query event, or <see langword="null"/> if
        /// the data does not pass the pre- or post-filtering logic.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the trace data cannot be decoded into a valid kernel registry query event.</exception>
        public JObject GetKernelRegistryQueryLog(TraceEvent data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // run pre filter function
            if (!this.PreFilterFunc(data))
            {
                return null;
            }

            log = KernelEventDecoder.DecodeKernelRegistryQueryEvent(data);
            if (log == null)
            {
                throw new ArgumentException(DebugMessages.TracepointAcceptErrMessage);
            }

            // run post filter function
            if (!this.PostFilterFunc(log))
            {
                return null;
            }

            return log;
        }
    }
}
