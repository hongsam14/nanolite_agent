using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nanolite_agent
{
    public static class SelfInfo
    {
        public static int PID { get; private set; }
        public static int ThreadID { get; private set; }
        public static string HostName { get; private set; }
        public static string UserName { get; private set; }

        static SelfInfo()
        {
            PID = System.Diagnostics.Process.GetCurrentProcess().Id;
            ThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            HostName = System.Environment.MachineName;
            UserName = System.Environment.UserName;
        }

    }
}
