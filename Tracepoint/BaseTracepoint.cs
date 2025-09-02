// <copyright file="BaseTracepoint.cs" company="Hongsam14">
// Copyright (c) Hongsam14. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Super class of Syslog Event log.
    /// </summary>
    public partial class BaseTracepoint
    {
        /// <summary>
        /// Delegate of PreFilter for Event log.
        /// <para>
        /// Filter Should be a function that takes a TraceEvent(specified by Microsoft.Diagnostics.Tracing),
        /// and returns a boolean.
        /// If the function returns true, the event will be logged.
        /// </para>
        /// <para>
        /// Although not mandatory, the naming convention for the Filter function is as follows.
        /// "<c>FilterBy</c>(filtering criteria)"
        /// </para>
        /// </summary>
        /// <param name="traceData">TraceEvent before parsed to json.</param>
        /// <returns>filter result.</returns>
        public delegate bool PreFilter(object traceData);

        /// <summary>
        /// Delegate of PostFilter for Event log.
        /// <para>
        /// Filter Should be a function that takes a JObject(Json),
        /// and returns a boolean.
        /// If the function returns true, the event will be logged.
        /// </para>
        /// <para>
        /// Although not mandatory, the naming convention for the Filter function is as follows.
        /// "<c>FilterBy</c>(filtering criteria)".
        /// </para>
        /// </summary>
        /// <param name="logData">json parsed log data.</param>
        /// <returns>filter result.</returns>
        public delegate bool PostFilter(JObject logData);


        /// <summary>
        /// Gets or Sets FilterFunc is the Delegate Chain of preFilter of the Event.
        /// </summary>
        /// <value>Property <c>FilterFunc</c> represents Delegate Chain of PreFilter of the Event.</value>
        public PreFilter PreFilterFunc { get; protected set; }

        /// <summary>
        /// Gets or Sets FilterFunc is the Delegate Chain of postFilter of the Event.
        /// </summary>
        /// <value>Property <c>FilterFunc</c> represents Delegate Chain of PostFilter of the Event.</value>
        public PostFilter PostFilterFunc { get; protected set; }

        /// <summary>
        /// Gets source of the event.
        /// </summary>
        /// <value>Property <c>Source</c> represents source of the event. </value>
        public string Source { get; protected set; }

        /// <summary>
        /// Gets AgentProcessID is the Process ID of the Agent's ProcessID.
        /// <value>Property <c>ProcessID</c> represents Process ID of the Agent.</value>
        public int AgentProcessID { get; private set; }

        /// <summary>
        /// Gets or Sets Regex for filtering UserName.
        /// </summary>
        protected Regex UserNameRegex { get; set; }

        /// <summary>
        /// Gets or Sets Regex for filtering agent Myself.
        /// </summary>
        protected Regex MyselfRegex { get; set; }
    }

    /// <summary>
    /// Super class of Syslog Event log.
    /// </summary>
    public partial class BaseTracepoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTracepoint"/> class.
        /// </summary>
        /// <param name="source">Syslog source in tracepoint.</param>
        public BaseTracepoint(string source)
        {
            this.Source = source;
            this.AgentProcessID = SelfInfo.PID;

            // Regex for filtering
            this.UserNameRegex = new Regex(@"^*\\SYSTEM");
            this.MyselfRegex = new Regex(@"^*\\nanolite-agent\.exe");

            // Default Prefilter
            this.PreFilterFunc += this.FilterByMySelfPID;
            this.PreFilterFunc += this.FilterBySystemProcessor;

            // Default PostFilter
            this.PostFilterFunc += this.FilterByMySelf;
        }

        /// <summary>
        /// Filtering func for Event log.
        /// It's prefiltering.
        /// <para>
        /// Filtering circuit delegate chain of FilterFunc and get results.
        /// </para>
        /// </summary>
        /// <param name="traceData">traceData is specified by Microsoft.Diagnostics.Tracing</param>
        /// <returns>"&" operation of all FilterFunc's delegate chain results.</returns>
        public bool PreFiltering(object traceData)
        {
            bool result = true;
            if (this.PreFilterFunc == null)
            {
                return result;
            }

            foreach (PreFilter val in (this.PreFilterFunc?.GetInvocationList()).Cast<PreFilter>())
            {
                result &= val(traceData);
            }

            return result;
        }

        /// <summary>
        /// Filtering func for Event log.
        /// It's postfiltering.
        /// </summary>
        /// <param name="logData">logData is serialized Json Object of Syslog.</param>
        /// <returns>"&" operation of all FilterFunc's delegate chain results.</returns>
        public bool PostFiltering(JObject logData)
        {
            bool result = true;
            if (this.PostFilterFunc == null)
            {
                return result;
            }

            foreach (PostFilter val in (this.PostFilterFunc?.GetInvocationList()).Cast<PostFilter>())
            {
                result &= val(logData);
            }

            return result;
        }

        /// <summary>
        /// This Filter function filters out the Agent itself by Process ID.
        /// </summary>
        /// <param name="traceData">Syslog from ETW Provider.</param>
        /// <returns>result of filtering.</returns>
        private bool FilterByMySelfPID(object traceData)
        {
            if (traceData == null)
            {
                throw new ArgumentNullException(paramName: "traceData");
            }

            TraceEvent realData = (TraceEvent)traceData;
            if (realData.ProcessID == SelfInfo.PID)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This Filter function filters out the System Processor by Process ID.
        /// </summary>
        /// <param name="traceData">ProcessTraceData specified by Microsoft.Diagnostics.Tracing.</param>
        /// <returns>result of filtering.</returns>
        private bool FilterBySystemProcessor(object traceData)
        {
            if (traceData == null)
            {
                throw new ArgumentNullException(paramName: "traceData");
            }

            TraceEvent realData = (TraceEvent)traceData;

            // filter out System Processor
            if (realData.ProcessID == 4)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This Filter function filters out the System Processor by UserName.
        /// </summary>
        /// <param name="logData">ProcessTraceData specified by Microsoft.Diagnostics.Tracing.</param>
        /// <returns>result of filtering.</returns>
        private bool FilterByUserName(JObject logData)
        {
            if (logData.ContainsKey("User") && this.UserNameRegex.IsMatch(logData["User"]?.ToString())) // System Processor
            {
                return false;
            }

            if (logData.ContainsKey("SourceUser") && this.UserNameRegex.IsMatch(logData["SourceUser"]?.ToString())) // System Processor
            {
                return false;
            }

            return true;
        }

        private bool FilterByMySelf(JObject logData)
        {
            JObject metadata = logData["Metadata"] as JObject;
            if (metadata.ContainsKey("Image") && this.MyselfRegex.IsMatch(metadata["Image"]?.ToString())) // Related with agent
            {
                return false;
            }

            if (metadata.ContainsKey("SourceImage") && this.MyselfRegex.IsMatch(metadata["SourceImage"]?.ToString())) // Related with agent
            {
                return false;
            }

            return true;
        }
    }
}
