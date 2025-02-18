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
            try
            {
                Nanolite_agent.Config.Config config = new Nanolite_agent.Config.Config("config.yml");
                Console.WriteLine($"CollectorIP: {config.CollectorIP} : CollectorPort {config.CollectorPort} : Exporter {config.Exporter}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

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

            Console.WriteLine(value: "Program terminated. Press any button");
            Console.ReadKey();
        }
    }
}
