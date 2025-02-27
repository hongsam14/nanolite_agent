
namespace Nanolite_agent.Tracepoint
{
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// kernel_process_creation:
    /// category: process_creation
    /// product: windows
    /// EventID: 1
    /// service: kernel-process.
    /// </summary>
    public class ProcessTerminate : Syslog
    {
        public ProcessTerminate()
            : base(2)
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
            if (!PostFilterFunc(jsonLog))
            {
                return null;
            }
            return jsonLog;
        }
    }
}
