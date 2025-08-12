// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.NanoException;
    using nanolite_agent.Properties;
    using Nanolite_agent.EventSession;

    /// <summary>
    /// Represents the entry point of the application, responsible for initializing configurations, starting monitoring
    /// sessions, and handling application lifecycle events.
    /// </summary>
    /// <remarks>This class initializes the necessary components, such as configuration, beacon, and event
    /// sessions, and manages the application's main execution flow. It also handles user interruptions (e.g., Ctrl+C)
    /// to gracefully stop monitoring and terminate the application.</remarks>
    internal static class Program
    {
        private static Config.Config config;
        private static Beacon.SystemActivityBeacon bcon;
        private static SystemActivity.SystemActivityRecorder systemRecorder;
        private static EventSession.SysmonEventSession sysmonSession;
        private static EventSession.KernelProcessEventSession kernelProcessSession;
        private static EventSession.KernelThreadEventSession kernelThreadSession;
        private static EventSession.KernelRegistryEventSession kernelRegistrySession;

        private static readonly string logo = @"
    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡀⠀⠀⣀⣀⡀⠀
    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣤⣶⣿⣽⣶⣾⣿⣿⣿⣿⠀
    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⠂⣰⣿⣿⡿⠟⠋⣿⣿⣿⣿⣿⣿⠏⠀
    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣶⣿⣣⣾⡿⠛⢉⣤⣶⣿⣿⣿⣿⣿⡿⠃⠀⠀
    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⣿⣿⣿⠟⢁⣤⣾⣿⣿⣿⣿⣿⣭⠥⠀⠀⠀⠀⠀
    ⠀⠀⠀⠀⠀⠀⠀⣠⣾⣿⣷⡿⠋⣀⣴⣿⣿⣿⣿⣿⣷⠌⠉⠁⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⠀⠀⢀⣼⣿⣿⣿⠟⢀⣼⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⠀⢀⣾⣿⣿⡿⠃⣰⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠰⣄⣾⣿⣿⡿⠁⣼⣿⣿⣿⣿⣿⡟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⣀⢻⣿⣿⡟⢀⣾⣿⢻⣿⠻⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠙⢿⣿⡿⠀⣾⣿⣿⠈⠟⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⠀⣿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⢰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⣼⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    ⠀⠀⠀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
     ____    ____  ____    ___   _      ____  ______    ___ 
    |    \  /    ||    \  /   \ | |    |    ||      |  /  _]
    |  _  ||  o  ||  _  ||     || |     |  | |      | /  [_ 
    |  |  ||     ||  |  ||  O  || |___  |  | |_|  |_||    _]
    |  |  ||  _  ||  |  ||     ||     | |  |   |  |  |   [_ 
    |  |  ||  |  ||  |  ||     ||     | |  |   |  |  |     |
    |__|__||__|__||__|__| \___/ |_____||____|  |__|  |_____|
    c) 2025 Nanolite Agent by shhong ENKI Corp)
    ";

        private static async Task Main(string[] args)
        {
            var cancelCompleted = new TaskCompletionSource();

            // print ascii logo with color yellow
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine(logo);
            Console.ResetColor();
            Console.OutputEncoding = Encoding.UTF8;

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
                    Exporter = Guid.NewGuid().ToString(),
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
            try
            {
                bcon = new Beacon.SystemActivityBeacon(in config);
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

            // Initialize SystemActivityRecorder
            try
            {
                systemRecorder = new SystemActivity.SystemActivityRecorder(in bcon);
            }
            catch (Exception e)
            {
                Console.WriteLine($"SystemActivityRecorder initialization failed: {e.Message}");
                return;
            }

            // start event sessions
            try
            {
                sysmonSession = new EventSession.SysmonEventSession(systemRecorder);
                kernelProcessSession = new EventSession.KernelProcessEventSession(systemRecorder);
                kernelThreadSession = new EventSession.KernelThreadEventSession(systemRecorder);
                kernelRegistrySession = new EventSession.KernelRegistryEventSession(systemRecorder);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Event session initialization failed: {e.Message}");
                return;
            }

            // Ctrl + C add event
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                Console.WriteLine("Ctrl + C pressed, stopping sessions and monitoring...");
                e.Cancel = true; // Prevent the process from terminating immediately
                _ = Task.Run(async () =>
                {
                    await CancelSequenceAsync();
                    cancelCompleted.SetResult();
                    Console.WriteLine("Monitoring stopped.");
                });
            };

            try
            {
                // Start Beacon
                bcon.StartMonitoring();

                // Start Session
                sysmonSession.StartSession();
                kernelProcessSession.StartSession();
                kernelThreadSession.StartSession();
                kernelRegistrySession.StartSession();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start event sessions {e.Message}");
            }

            Console.WriteLine("Press Ctrl + C to stop monitoring and exit...");

            try
            {
                // Wait Session.
                sysmonSession.WaitSession();
                kernelProcessSession.WaitSession();
                kernelThreadSession.WaitSession();
                kernelRegistrySession.WaitSession();

                await cancelCompleted.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception raised while wait session: {ex.Message} {ex.StackTrace}");
            }

            Console.WriteLine(value: DebugMessages.ExitMessage);
        }

        private static async Task CancelSequenceAsync()
        {
            // Stop all sessions
            kernelRegistrySession.StopSession();
            kernelThreadSession.StopSession();
            kernelThreadSession.StopSession();
            kernelProcessSession.StopSession();
            sysmonSession.StopSession();

            // flush all activities
            try
            {
                systemRecorder.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error flushing actors: {e.Message}");
            }

            // Stop Beacon
            bcon.StopMonitoring();
        }
    }
}
