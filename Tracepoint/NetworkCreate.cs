﻿// <copyright file="NetworkCreate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json.Linq;

    public class NetworkCreate : Syslog
    {
        public NetworkCreate()
            : base(3)
        {
        }

        public override JObject EventLog(TraceEvent traceData)
        {
            JObject jsonLog;

            if (!this.PreFilterFunc(traceData))
            {
                return null;
            }
            jsonLog = base.EventLog(traceData);
            for (int i = 0; i < traceData.PayloadNames.Length; i++)
            {
                jsonLog.Add(traceData.PayloadNames[i], traceData.PayloadValue(i).ToString());
            }
            if (!this.PostFilterFunc(jsonLog))
            {
                return null;
            }
            return jsonLog;
        }
    }
}
