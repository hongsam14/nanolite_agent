// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent
{
    using System;
    using Microsoft.Diagnostics.Tracing.Session;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // check if the program is running as an administrator
            if (!TraceEventSession.IsElevated() ?? false)
            {
                Console.WriteLine(value: "Run as Administrator");
                return;
            }

            // check SelfInfo class is initialized.
            Console.WriteLine($"Self PID: {SelfInfo.PID} : ThreadId {SelfInfo.ThreadID}");

            // get config from config.yml
            Config.Config config = null;
#if !DEBUG
            try
            {
                config = new Nanolite_agent.Config.Config("config.yml");
                Console.WriteLine($"CollectorIP: {config.CollectorIP} : CollectorPort {config.CollectorPort} : Exporter {config.Exporter}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
#endif

            // Init Beacon
            Beacon.Beacon bcon = new Beacon.Beacon(config);

            // Initialize the ETW session
            EventSession.ProcessEventSession procEventSession = new EventSession.ProcessEventSession(bcon);
            //EventSession.NetworkEventSession netEventSession = new EventSession.NetworkEventSession(bcon);

            // Ctrl + C add event
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                //netEventSession.StopSession();
                procEventSession.StopSession();
                bcon.Stop();
            };


            // Start Session
            procEventSession.StartSession();
            //netEventSession.StartSession();

            // Wait Session.
            procEventSession.WaitSession();
            //netEventSession.WaitSession();

            Console.WriteLine(value: "Program terminated. Press any button");
            Console.ReadKey();
        }
    }
}
