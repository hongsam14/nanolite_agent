// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent
{
    using System;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.NanoException;
    using nanolite_agent.Properties;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Config.Config config;
            Beacon.Beacon bcon;

            // check if the program is running as an administrator
            if (!TraceEventSession.IsElevated() ?? false)
            {
                Console.WriteLine(value: DebugMessages.PrivilegeMessage);
                return;
            }

            // check SelfInfo class is initialized.
            Console.WriteLine($"Self PID: {SelfInfo.PID} : ThreadId {SelfInfo.ThreadID}");

            // get config from config.yml
            config = null;
            try
            {
#if DEBUG
                // init dummy config for debug
                Config.ConfigWrapper dummyConfigWrapper = new Config.ConfigWrapper
                {
                    CollectorIP = "localhost",
                    CollectorPort = "4317",
                    Exporter = "TestBed",
                };
                config = new Config.Config(dummyConfigWrapper);
#else
                // get config file path from the current directory
                config = Config.ConfigExtension.LoadConfigFromPath("config.yml");
#endif
                Console.WriteLine($"CollectorIP: {config.CollectorIP} : CollectorPort {config.CollectorPort} : Exporter {config.Exporter}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            // Init Beacon
            bcon = new Beacon.Beacon(config);

#if DEBUG
            // Initialize the ETW session
            EventSession.SysmonEventSession sysmonSession = new EventSession.SysmonEventSession();
#else
            EventSession.SysmonEventSession sysmonSession = new EventSession.SysmonEventSession(bcon);
#endif

            // Ctrl + C add event
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                sysmonSession.StopSession();
                //bcon.Stop();
            };


            // Start Session
            sysmonSession.StartSession();

            // Wait Session.
            sysmonSession.WaitSession();

            Console.WriteLine(value: DebugMessages.ExitMessage);
            Console.ReadKey();
        }
    }
}
