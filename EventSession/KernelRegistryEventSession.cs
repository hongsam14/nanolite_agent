// <copyright file="KernelRegistryEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Helper;
    using Nanolite_agent.NanoException;
    using Nanolite_agent.SystemActivity;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Manages a kernel-mode ETW (Event Tracing for Windows) session to monitor registry-related events.
    /// </summary>
    /// <remarks>This class provides functionality to start, stop, and wait for a kernel-mode ETW session. It
    /// listens for registry query events and uses a <see cref="SystemActivityBeacon"/> to log these
    /// events. The session is configured to automatically stop when disposed and uses a buffer size of 1024
    /// MB.</remarks>
    public class KernelRegistryEventSession : IEventSession
    {
        /// <summary>
        /// The name of the ETW (Event Tracing for Windows) session.
        /// </summary>
        /// <remarks>This field holds the default name for the ETW session used in tracing operations. It
        /// is a constant value and cannot be modified.</remarks>
        private readonly string sessionName = "kernel_etw_registry_session";

        private readonly TraceEventSession traceEventSession;
        private readonly ETWKernel etwKernelTracepoint;
        private readonly SystemActivityRecorder sysActRecorder;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="KernelRegistryEventSession"/> class,  which manages an ETW
        /// (Event Tracing for Windows) session for monitoring kernel-level events.
        /// </summary>
        /// <remarks>This constructor sets up an ETW session with a default buffer size of 1024 MB and
        /// ensures  that the session is stopped when disposed. It also initializes kernel tracepoints and  subscribes
        /// to relevant ETW providers for event monitoring.</remarks>
        /// <param name="sysActRecorder">An instance of <see cref="SystemActivityRecorder"/> used to record system activity.  This parameter cannot
        /// be <see langword="null"/>.</param>
        public KernelRegistryEventSession(SystemActivityRecorder sysActRecorder)
        {
            // null check for Beacon
            ArgumentNullException.ThrowIfNull(sysActRecorder);
            this.sysActRecorder = sysActRecorder;

            // initialize etw session
            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize Kernel Process Tracepoint
            this.etwKernelTracepoint = new Tracepoint.ETWKernel();

            // subscribe function to etw session
            this.SubscribeProvider();
            this.RegisterCallback();
        }

        /// <summary>
        /// This method checks if a registry event is interesting based on the key name and process name.
        /// The source of these rules is from SwiftOnSecurity's Sysmon config repository.
        /// https://github.com/SwiftOnSecurity/sysmon-config.
        /// </summary>
        /// <param name="keyName">The registry key name.</param>
        /// <param name="processName">The image name of path of process.</param>
        /// <returns>boolean result that event is interesting.</returns>
        public static bool IsInterestingRegistryEvent(string keyName, string processName)
        {
            string target = keyName?.ToLowerInvariant() ?? string.Empty;
            string processNameLower = processName?.ToLowerInvariant() ?? string.Empty;

            // Exclusion rules (noise filtering)
            if (
                target.Contains(@"\{cafeefac-") ||
                target.StartsWith(@"hklm\components") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\appmodel\staterepository\cache") ||
                target.EndsWith(@"\toolbar\webbrowser") ||
                target.EndsWith(@"\browser\itbar7height") ||
                target.EndsWith(@"\browser\itbar7layout") ||
                target.EndsWith(@"internet explorer\toolbar\locked") ||
                target.EndsWith(@"\toolbar\webbrowser\{47833539-d0c5-4125-9fa8-0819e2eaac93}") ||
                target.EndsWith(@"}\previouspolicyareas") ||
                target.Contains(@"\control\wmi\autologger\") ||
                target == @"hklm\system\currentcontrolset\services\usosvc\start" ||
                target.EndsWith(@"\lsass\offlinejoin\currentvalue") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\installer\userdata\s-1-5-18\") ||
                target.Contains(@"_classes\appx") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\winevt\publishers\") ||
                target.EndsWith(@"\lsapid") ||
                target.EndsWith(@"\sspicache") ||
                target.EndsWith(@"\kerberos\domains") ||
                target.EndsWith(@"\bits\start") ||
                target.EndsWith(@"\clr_optimization_v2.0.50727_32\start") ||
                target.EndsWith(@"\clr_optimization_v2.0.50727_64\start") ||
                target.EndsWith(@"\clr_optimization_v4.0.30319_32\start") ||
                target.EndsWith(@"\clr_optimization_v4.0.30319_64\start") ||
                target.EndsWith(@"\deviceassociationservice\start") ||
                target.EndsWith(@"\fhsvc\start") ||
                target.EndsWith(@"\nal\start") ||
                target.EndsWith(@"\trustedinstaller\start") ||
                target.EndsWith(@"\tunnel\start") ||
                target.EndsWith(@"\usosvc\start") ||
                target.EndsWith(@"\userchoice\progid") ||
                target.EndsWith(@"\userchoice\hash") ||
                target.EndsWith(@"\openwithlist\mrulist") ||
                target.Contains("shell extentions\\cached") ||
                target.EndsWith(@"\audit\specialgroups") ||
                target.EndsWith(@"\scripts\startup\0\psscriptorder") ||
                target.EndsWith(@"\scripts\startup\0\som-id") ||
                target.EndsWith(@"\scripts\startup\0\gpo-id") ||
                target.EndsWith(@"\scripts\startup\0\0\ispowershell") ||
                target.EndsWith(@"\scripts\startup\0\0\exectime") ||
                target.EndsWith(@"\scripts\shutdown\0\psscriptorder") ||
                target.EndsWith(@"\scripts\shutdown\0\som-id") ||
                target.EndsWith(@"\scripts\shutdown\0\gpo-id") ||
                target.EndsWith(@"\scripts\shutdown\0\0\ispowershell") ||
                target.EndsWith(@"\scripts\shutdown\0\0\exectime") ||
                target.Contains(@"\safer\codeidentifiers\0\hashes\{") ||
                target.Contains(@"virtualstore\machine\software\microsoft\office\clicktorun\") ||
                target.StartsWith(@"hklm\software\microsoft\office\clicktorun\") ||
                processNameLower.EndsWith(@"btwdins.exe") ||
                target.StartsWith(@"hkcr\vlc.") ||
                target.StartsWith(@"hkcr\itunes.") ||
                target == @"hklm\software\microsoft\windows\currentversion\winevt\publishers\{945a8954-c147-4acd-923f-40c45405a658}")
            {
                return false; // exclude
            }

            // Inclusion rules (detection triggers)
            if (
                target.Contains(@"currentversion\run") ||
                target.Contains(@"policies\explorer\run") ||
                target.Contains(@"group policy\scripts") ||
                target.Contains(@"windows\system\scripts") ||
                target.Contains(@"currentversion\windows\load") ||
                target.Contains(@"currentversion\winlogon\shell") ||
                target.Contains(@"currentversion\winlogon\system") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\winlogon\notify") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\winlogon\shell") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\winlogon\userinit") ||
                target.StartsWith(@"hklm\software\wow6432node\microsoft\windows nt\currentversion\drivers32") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\session manager\bootexecute") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\aedebug") ||
                target.Contains("userinitmprlogonscript") ||
                target.EndsWith("user shell folders\\startup") ||
                target.EndsWith(@"\servicedll") ||
                target.EndsWith(@"\servicemanifest") ||
                target.EndsWith(@"\imagepath") ||
                target.EndsWith(@"\start") ||
                target.EndsWith(@"rdp-tcp\portnumber") ||
                target.EndsWith(@"control\terminal server\fSingleSessionPerUser") ||
                target.EndsWith(@"fdenytsconnections") ||
                target.EndsWith("lastloggedonuser") ||
                target.Contains(@"\command\") ||
                target.Contains(@"\ddeexec\") ||
                target.Contains(@"{86c86720-42a0-1069-a2e8-08002b30309d}") ||
                target.Contains("exefile") ||
                target.EndsWith(@"\inprocserver32\(default)") ||
                target.EndsWith(@"\hidden") ||
                target.EndsWith(@"\showsuperhidden") ||
                target.EndsWith(@"\hidefileext") ||
                target.Contains(@"classes\*\") ||
                target.Contains(@"classes\allfilesystemobjects\") ||
                target.Contains(@"classes\directory\") ||
                target.Contains(@"classes\drive\") ||
                target.Contains(@"classes\folder\") ||
                target.Contains(@"classes\protocols\") ||
                target.Contains(@"contextmenuhandlers\") ||
                target.Contains(@"currentversion\shell") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\explorer\shellexecutehooks") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\explorer\shellserviceobjectdelayload") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\explorer\shelliconoverlayidentifiers") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\app paths\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\terminal server\winstations\rdp-tcp\initialprogram") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\winlogon\gpextensions\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\services\winsock") ||
                target.EndsWith(@"\proxyserver") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\authentication\credential provider") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\lsa\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\securityproviders") ||
                target.StartsWith(@"hklm\software\microsoft\netsh") ||
                target.Contains(@"internet settings\proxyenable") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\networkprovider\order\") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\networklist\profiles") ||
                target.EndsWith(@"\enablefirewall") ||
                target.EndsWith(@"\donotallowexceptions") ||
                target.StartsWith(@"hklm\system\currentcontrolset\services\sharedaccess\parameters\firewallpolicy") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\windows\appinit_dlls\") ||
                target.StartsWith(@"hklm\software\wow6432node\microsoft\windows nt\currentversion\windows\appinit_dlls\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\session manager\appcertdlls\") ||
                target.Contains(@"outlook\addins\") ||
                target.Contains(@"office test\") ||
                target.Contains(@"trustrecords") ||
                target.EndsWith(@"\enablebho") ||
                target.Contains(@"toolbar\") ||
                target.Contains(@"extensions\") ||
                target.Contains(@"browser helper objects\") ||
                target.EndsWith(@"\disablesecuritysettingscheck") ||
                target.EndsWith(@"\3\1206") ||
                target.EndsWith(@"\3\2500") ||
                target.EndsWith(@"\3\1809") ||
                target.StartsWith(@"hklm\software\classes\clsid\{ab8902b4-09ca-4bb6-b78d-a8f59079a8d5}\") ||
                target.StartsWith(@"hklm\software\classes\wow6432node\clsid\{ab8902b4-09ca-4bb6-b78d-a8f59079a8d5}\") ||
                target.StartsWith(@"hklm\software\classes\clsid\{083863f1-70de-11d0-bd40-00a0c911ce86}\") ||
                target.StartsWith(@"hklm\software\classes\wow6432node\clsid\{083863f1-70de-11d0-bd40-00a0c911ce86}\") ||
                target.EndsWith(@"\urlupdateinfo") ||
                target.EndsWith(@"\installsource") ||
                target.EndsWith(@"\eulaaccepted") ||
                target.EndsWith(@"\disableantispyware") ||
                target.EndsWith(@"\disableantivirus") ||
                target.EndsWith(@"\spynetreporing") ||
                target.Contains("disablerealtimemonitoring") ||
                target.StartsWith(@"hklm\software\policies\microsoft\windows defender\exclusions\") ||
                target.EndsWith(@"\enablelua") ||
                target.EndsWith(@"\localaccounttokenfilterpolicy") ||
                target.EndsWith(@"\hideSCAHealth") ||
                target.StartsWith(@"hklm\software\microsoft\security center\") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\appcompatflags\custom") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\appcompatflags\installedsdb") ||
                target.Contains("virtualstore") ||
                target.StartsWith(@"hklm\software\microsoft\windows nt\currentversion\image file execution options\") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\winevt\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\safeboot\") ||
                target.StartsWith(@"hklm\system\currentcontrolset\control\winlogon\") ||
                target.EndsWith(@"\friendlyname") ||
                target == @"hklm\software\microsoft\windows\currentversion\installer\inprogress\(default)" ||
                target.StartsWith(@"hklm\software\microsoft\tracing\rasapi32") ||
                target.StartsWith(@"hklm\software\microsoft\windows\currentversion\capabilityaccessmanager\consentstore\") ||
                target.Contains(@"\keyboard layout\preload") ||
                target.Contains(@"\keyboard layout\substitutes") ||
                target.EndsWith(@"\lowercaselongpath") ||
                target.EndsWith(@"\publisher") ||
                target.EndsWith(@"\binproductversion") ||
                target.EndsWith(@"\driverversion") ||
                target.EndsWith(@"\driververversion") ||
                target.EndsWith(@"\linkdate") ||
                target.Contains(@"compatibility assistant\store\") ||
                processNameLower.EndsWith("regedit.exe") ||
                processName.StartsWith(@"\"))
            {
                return true; // matched include
            }

            return false; // default deny
        }

        /// <summary>
        /// Starts a new session for processing trace events asynchronously.
        /// </summary>
        /// <remarks>This method begins processing trace events on a background task.  The session will
        /// continue running until explicitly stopped. Ensure that the session is properly stopped to release
        /// resources.</remarks>
        public void StartSession()
        {
            this.sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this.traceEventSession?.Source.Process();
            });
        }

        /// <summary>
        /// Stops the current trace event session, if one is active.
        /// </summary>
        /// <remarks>This method halts the trace event session associated with the instance.  If no
        /// session is active, the method performs no action.</remarks>
        public void StopSession()
        {
            this.traceEventSession?.Stop();
        }

        /// <summary>
        /// Waits for the current session task to complete.
        /// </summary>
        /// <remarks>This method blocks the calling thread until the session task, initiated by <see
        /// cref="StartSession"/>, completes. Ensure that <see cref="StartSession"/> has been called before invoking
        /// this method.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session task is not running. Call <see cref="StartSession"/> before calling this method.</exception>
        public void WaitSession()
        {
            if (this.sessionTask == null)
            {
                throw new InvalidOperationException("sessonTask is not running. Call StartSession before calling WaitSession");
            }

            this.sessionTask.Wait();
        }

        private void SubscribeProvider()
        {
            this.traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.Registry);
        }

        private void RegisterCallback()
        {
            this.traceEventSession.Source.Kernel.RegistryOpen += this.RegistryQuery;
        }

        private void RegistryQuery(RegistryTraceData eventData)
        {
            JObject syslog;

            // null check for sysActRecorder
            ArgumentNullException.ThrowIfNull(eventData);

            // check if process id is existing in sysActRecorder
            if (!this.sysActRecorder.IsProcessTracked(eventData.ProcessID))
            {
                return; // process id not found, skip
            }

            // filter key name and image
            if (!KernelRegistryEventSession.IsInterestingRegistryEvent(eventData.KeyName, eventData.ProcessName))
            {
                return; // not interesting, skip
            }

            syslog = this.etwKernelTracepoint.GetKernelRegistryQueryLog(eventData);
            if (syslog == null)
            {
                // this case means syslog is filtered out by etwKernelTracepoint
                return;
            }

            try
            {
                this.sysActRecorder.RecordProcessAction(
                    eventData.ProcessID,
                    eventData.ProcessName,
                    SysEventCode.RegistryQuery,
                    syslog);
            }
            catch (SystemActivityException ex)
            {
                // log the exception
                Console.WriteLine($"Error processing kernel registry query event: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new NanoException.BeaconException(
                    "Error processing kernel registry query event",
                    ex);
            }
        }
    }
}
