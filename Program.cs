﻿// <copyright file="Program.cs" company="PlaceholderCompany">
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
            Beacon.SystemActivityBeacon bcon;
            EventSession.SysmonEventSession sysmonSession;

            // check if the program is running as an administrator
            if (!TraceEventSession.IsElevated() ?? false)
            {
                Console.WriteLine(value: DebugMessages.PrivilegeMessage);
                return;
            }

            // check SelfInfo class is initialized.
            Console.WriteLine($"Self PID: {SelfInfo.PID} : ThreadId {SelfInfo.ThreadID}");

            // get config from config.yml
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
#if DEBUG
            // start event sessions
            sysmonSession = new EventSession.SysmonEventSession();
#else

            // Init Beacon
            try
            {
                bcon = new Beacon.SystemActivityBeacon(config);
                bcon.StartMonitoring();
            }
            catch (BeaconException be)
            {
                Console.WriteLine(be.Message);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Beacon initialization failed: {e.Message}");
                return;
            }
            // start event sessions
            sysmonSession = new EventSession.SysmonEventSession(bcon);
#endif

            // Ctrl + C add event
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                sysmonSession.StopSession();
#if !DEBUG
                bcon.StopMonitoring();
#endif
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
