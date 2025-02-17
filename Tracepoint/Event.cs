using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Diagnostics.Tracing;

using Newtonsoft.Json.Linq;

namespace nanolite_agent.Tracepoint
{
    /// <summary>
    /// Super class of Event log.
    /// </summary>
    public class Event
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
        /// <param>TraceEvent</param>
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
        /// "<c>FilterBy</c>(filtering criteria)"
        /// </para>
        /// </summary>
        /// <param>logData</param>
        public delegate bool PostFilter(JObject logData);

        /// <value>Property <c>EventID</c> represents ID of the Event.</value>
        public int EventID { get; }

        /// <value>Property <c>FilterFunc</c> represents Delegate Chain of PreFilter of the Event.</value>
        public PreFilter PreFilterFunc { get; protected set; }

        /// <value>Property <c>FilterFunc</c> represents Delegate Chain of PostFilter of the Event.</value>
        public PostFilter PostFilterFunc { get; protected set; }

        /// <value>Property <c>ProcessID</c> represents Process ID of the Agent.</value>
        public int ProcessID { get; private set; }

        protected Regex userNameRegex;
        protected Regex myselfRegex;

        // ttps://learn.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-8.0
        // TODO: Add gRPC client, dependency injection
        public Event(int eventID)
        {
            this.EventID = eventID;
            this.ProcessID = SelfInfo.PID;
            // Regex for filtering
            userNameRegex = new Regex(@"^*\\SYSTEM");
            myselfRegex = new Regex(@"^*\\googoo_agent\.exe");
            // Default Prefilter
            this.PreFilterFunc += FilterByMySelfPID;
            this.PreFilterFunc += FilterBySystemProcessor;
            // Default PostFilter
            //this.PostFilterFunc += FilterByUserName;
            this.PostFilterFunc += FilterByMySelf;
        }

        /// <summary>
        /// This Filter function filters out the System Processor by Process ID.
        /// </summary>
        /// <param name="traceData">ProcessTraceData specified by Microsoft.Diagnostics.Tracing</param>
        /// <returns>result of filtering</returns>
        private bool FilterBySystemProcessor(object traceData)
        {
            TraceEvent realData = (TraceEvent)traceData;

            if (realData.ProcessID == 4) // System Processor
                return false;
            return true;
        }
        public bool FilterByMySelfPID(object traceData)
        {
            TraceEvent realData = (TraceEvent)traceData;
            if (realData.ProcessID == SelfInfo.PID)
                return false;
            return true;
        }

        private bool FilterByUserName(JObject logData)
        {
            if (logData.ContainsKey("User") && userNameRegex.IsMatch(logData["User"]?.ToString())) // System Processor
                return false;
            if (logData.ContainsKey("SourceUser") && userNameRegex.IsMatch(logData["SourceUser"]?.ToString())) // System Processor
                return false;
            return true;
        }

        private bool FilterByMySelf(JObject logData)
        {
            if (logData.ContainsKey("Image") && myselfRegex.IsMatch(logData["Image"]?.ToString())) // Related with agent
                return false;
            if (logData.ContainsKey("SourceImage") && myselfRegex.IsMatch(logData["SourceImage"]?.ToString())) // Related with agent
                return false;
            return true;
        }

        /// <summary>
        /// Filtering func for Event log.
        /// <para>
        /// Filtering circuit delegate chain of FilterFunc and get results.
        /// </para>
        /// </summary>
        /// <param name="traceData">traceData is specified by Microsoft.Diagnostics.Tracing</param>
        /// <returns>"&" operation of all FilterFunc's results.</returns>
        public bool PreFiltering(object traceData)
        {
            bool result = true;
            if (PreFilterFunc == null)
                return result;
            foreach (PreFilter val in (PreFilterFunc?.GetInvocationList()).Cast<PreFilter>())
            {
                result &= val(traceData);
            }
            return result;
        }
        public bool PostFiltering(JObject logData)
        {
            bool result = true;
            if (PostFilterFunc == null)
                return result;
            foreach (PostFilter val in (PostFilterFunc?.GetInvocationList()).Cast<PostFilter>())
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
                //new JProperty("EventID", this.EventID),
                new JProperty("EventID", data.ID),
                new JProperty("EventName", data.EventName),
                new JProperty("UsermodePid", data.ProcessID),
                new JProperty("TimeStamp", data.TimeStamp)
                //new JProperty("Opcode", data.Opcode)
                //new JProperty("user", commonEventHeader?.userName ?? "NOT_REACHABLE"),
                //new JProperty("image", commonEventHeader?.image ?? "NOT_REACHABLE"),
                //new JProperty("TaskGuid", data.TaskGuid),
                //new JProperty("workingDirectory", commonEventHeader?.workingDirectory ?? "NOT_REACHABLE"),
                );
            return jsonLog;
        }
    }
}
