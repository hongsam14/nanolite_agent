using System;
using Microsoft.Diagnostics.Tracing.Session;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nanolite_agent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!(TraceEventSession.IsElevated()) ?? false)
            {
                Console.WriteLine("Run as Administrator");
                return;
            }
            //Initialize the gRPC Client
            Console.WriteLine($"Self PID: {SelfInfo.PID} : ThreadId {SelfInfo.ThreadID}");
            // Initialize the ETW session
            EventSession.ProcessEventSession procEventSession = new EventSession.ProcessEventSession();

            // Ctrl + C add event
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                procEventSession.StopSession();
            };

            // Start Session
            procEventSession.StartSession();
            // Wait Session.
            procEventSession.WaitSession();

            Console.WriteLine("프로그램이 종료되었습니다. 콘솔 창을 닫으려면 아무 키나 누르세요.");
            Console.ReadKey();
        }
    }
}
