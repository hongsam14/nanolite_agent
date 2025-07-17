// <copyright file="Sysmon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Nanolite_agent.Helper;
    using Newtonsoft.Json.Linq;

    public class Sysmon : BaseTracepoint
    {
        public Sysmon()
            : base("sysmon")
        {
        }

        public JObject GetSysmonLog(TraceEvent data)
        {
            JObject log;

            // check data is not null
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            log = SysmonEventDecoder.DecodeSysmonEvent(data);
            if (log == null)
            {
                throw new ArgumentException("trace event data is not acceptable");
            }

            return log;
        }
    }
}
