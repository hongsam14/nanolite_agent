// <copyright file="ProcessPipeline.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace Nanolite_agent.Pipeline
{
    using System.Collections.Generic;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Newtonsoft.Json;

    /// <summary>
    /// ProcessPipeline class of the Nanolite agent.
    /// </summary>
    public class ProcessPipeline
    {
        private readonly Tracepoint.ProcessCreate _processCreateTracepoint;

        // TODO: boolean should be replaced with Otel Span
        private Dictionary<long, bool> _processSpanMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessPipeline"/> class.
        /// </summary>
        public ProcessPipeline()
        {
            this._processCreateTracepoint = new Tracepoint.ProcessCreate();
            this._processSpanMap = new Dictionary<long, bool>();
        }

        public void ProcessCreateHooker(ProcessTraceData traceData)
        {
            var log = this._processCreateTracepoint.EventLog(traceData);
            // preprocess filtering
            if (log == null)
            {
                return;
            }
#if DEBUG
            System.Console.WriteLine(JsonConvert.SerializeObject(log));
#endif
        }
    }
}
