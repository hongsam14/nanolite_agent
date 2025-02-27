// <copyright file="Event.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Tracepoint
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Super class of Syslog Event log.
    /// </summary>
    public partial class Syslog
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
        /// Gets EventID is the ID of the Syslog Event.
        /// </summary>
        /// <value>Property <c>EventID</c> represents ID of the Event.</value>
        public int EventID { get; }

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
        /// Gets ProcessID is the Process ID of the Syslog Event.
        /// <value>Property <c>ProcessID</c> represents Process ID of the Agent.</value>
        public int ProcessID { get; private set; }

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
    public partial class Syslog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syslog"/> class.
        /// </summary>
        /// <param name="eventID">Syslog Event Id in ETW provider.</param>
        public Syslog(int eventID)
        {
            this.EventID = eventID;
            this.ProcessID = SelfInfo.PID;

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
        /// This Filter function filters out the Agent itself by Process ID.
        /// </summary>
        /// <param name="traceData">Syslog from ETW Provider.</param>
        /// <returns>result of filtering.</returns>
        public bool FilterByMySelfPID(object traceData)
        {
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
            if (logData.ContainsKey("Image") && this.MyselfRegex.IsMatch(logData["Image"]?.ToString())) // Related with agent
            {
                return false;
            }

            if (logData.ContainsKey("SourceImage") && this.MyselfRegex.IsMatch(logData["SourceImage"]?.ToString())) // Related with agent
            {
                return false;
            }

            return true;
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
        /// Super Event log for Event logging.
        /// </summary>
        /// <param name="data">trace data specified by Microsoft.Diagnotics.Tracing</param>
        /// <returns>returns log formatted in json</returns>
        public virtual JObject EventLog(TraceEvent data)
        {
            //CommonEventHeader? commonEventHeader = CommonEventHeader.GetCommonEventHeader(data.ProcessID);
            JObject jsonLog = new JObject(
                new JProperty("EventID", data.ID),
                new JProperty("EventName", data.EventName),
                new JProperty("UsermodePid", data.ProcessID),
                new JProperty("TimeStamp", data.TimeStamp));
                //new JProperty("Opcode", data.Opcode)
                //new JProperty("user", commonEventHeader?.userName ?? "NOT_REACHABLE"),
                //new JProperty("image", commonEventHeader?.image ?? "NOT_REACHABLE"),
                //new JProperty("TaskGuid", data.TaskGuid),
                //new JProperty("workingDirectory", commonEventHeader?.workingDirectory ?? "NOT_REACHABLE"),
            return jsonLog;
        }
    }
}
