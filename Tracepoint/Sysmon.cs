// <copyright file="Sysmon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Nanolite_agent.Helper;
    using nanolite_agent.Properties;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a tracepoint for Sysmon events.
    /// </summary>
    public class Sysmon : BaseTracepoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sysmon"/> class.
        /// </summary>
        public Sysmon()
            : base("sysmon")
        {
        }

        /// <summary>
        /// Gets the Sysmon log as a <see cref="JObject"/> from the specified <see cref="TraceEvent"/>.
        /// </summary>
        /// <param name="data">The trace event data to decode.</param>
        /// <returns>
        /// A <see cref="JObject"/> representing the Sysmon log, or <c>null</c> if the event does not pass filters.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the event cannot be decoded.</exception>
        public JObject GetSysmonLog(TraceEvent data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // run pre filter function
            //if (!this.PreFilterFunc(data))
            //{
            //    return null;
            //}

            log = SysmonEventDecoder.DecodeSysmonEvent(data);
            if (log == null)
            {
                throw new ArgumentException(DebugMessages.TracepointAcceptErrMessage);
            }

            // run post filter function
            //if (!this.PostFilterFunc(log))
            //{
            //    return null;
            //}

            return log;
        }
    }
}
