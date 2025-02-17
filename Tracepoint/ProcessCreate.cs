using Microsoft.Diagnostics.Tracing;
using Newtonsoft.Json.Linq;

namespace nanolite_agent.Tracepoint
{
    /// <summary>
    /// kernel_process_creation:
    /// category: process_creation
    /// product: windows
    /// conditions:
    /// EventID: 1
    /// rewrite:
    /// product: windows
    /// service: kernel-process
    /// </summary>
    public class ProcessCreate : Event
    {
        public ProcessCreate() : base(1)
        { }

        public override JObject EventLog(TraceEvent traceData)
        {
            JObject jsonLog;

            if (!PreFilterFunc(traceData))
            {
                return null;
            }
            jsonLog = base.EventLog(traceData);
            for (int i = 0; i < traceData.PayloadNames.Length; i++)
            {
                jsonLog.Add(traceData.PayloadNames[i], traceData.PayloadValue(i).ToString());
            }
            if (!PostFilterFunc(jsonLog))
            {
                return null;
            }

#if DEBUG
            System.Console.WriteLine(jsonLog);
#endif
            return jsonLog;
        }
    }
}
