// <copyright file="SysmonEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;
    using Nanolite_agent.Beacon;
    using Nanolite_agent.Helper;
    using Nanolite_agent.NanoException;
    using Nanolite_agent.SystemActivity;
    using Nanolite_agent.Tracepoint;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a session for monitoring system activity using Sysmon events.
    /// </summary>
    /// <remarks>This class provides functionality to manage a trace event session for capturing and
    /// processing Sysmon events. It allows starting, stopping, and waiting for the session, as well as handling event
    /// callbacks for various Sysmon event types. The session is automatically configured to stop when
    /// disposed.</remarks>
    public sealed class SysmonEventSession : IEventSession
    {
        /// <summary>
        /// Gets the name of the session used for monitoring system events.
        /// </summary>
        private readonly string sessionName = "Microsoft-Windows-Sysmon";

        /// <summary>
        /// Gets the unique identifier for the provider.
        /// </summary>
        private readonly string providerGUID = "5770385f-c22a-43e0-bf4c-06f5698ffbd9";

        private readonly TraceEventSession traceEventSession;
        private readonly Sysmon sysmonTracepoint;
        private readonly SystemActivityRecorder sysActRecorder;
        private Task sessionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="SysmonEventSession"/> class,  which manages a session for
        /// capturing and processing system activity events using Sysmon.
        /// </summary>
        /// <remarks>This constructor sets up the necessary components for capturing Sysmon events,
        /// including initializing a trace event session and subscribing to the appropriate event providers. The session
        /// is configured to automatically stop when disposed, and it uses a buffer size of 1024 MB.</remarks>
        /// <param name="sysrecorder">An instance of <see cref="SystemActivityRecorder"/> used to record and process system activity events.</param>
        public SysmonEventSession(SystemActivityRecorder sysrecorder)
        {
            // null check of sysrecorder
            ArgumentNullException.ThrowIfNull(sysrecorder);
            this.sysActRecorder = sysrecorder;

            // initialize TraceEventSession
            this.traceEventSession = new TraceEventSession(this.sessionName)
            {
                StopOnDispose = true,
                BufferSizeMB = 1024,
            };

            this.sessionTask = null;

            // initialize Sysmon Tracepoint
            this.sysmonTracepoint = new Tracepoint.Sysmon();

            // subscribe function to etw session
            this.SubscribeProvider();
            this.RegisterCallback();
        }

        /// <summary>
        /// Starts a new session and begins processing events asynchronously.
        /// </summary>
        /// <remarks>This method initiates the session and processes events in a background task.  The
        /// session will continue running until explicitly stopped.</remarks>
        public void StartSession()
        {
            // Start the session and process events in a separate task
            this.sessionTask = Task.Run(() =>
            {
                // blocked until stop is called.
                this.traceEventSession?.Source.Process();
            });
        }

        /// <summary>
        /// Stops the current trace event session and halts beacon monitoring.
        /// </summary>
        /// <remarks>This method terminates the active trace event session if one is running.  It is safe
        /// to call this method multiple times; subsequent calls will have no effect  if the session has already been
        /// stopped.</remarks>
        public void StopSession()
        {
            // Stop the session and the beacon monitoring
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
            this.traceEventSession.EnableProvider(this.providerGUID);
        }

        private void RegisterCallback()
        {
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "ProcessCreate(rule:ProcessCreate)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processaccessed(rule:ProcessAccess)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Processterminated(rule:ProcessTerminate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "ProcessTampering(rule:ProcessTampering)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "CreateRemoteThreaddetected(rule:CreateRemoteThread)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "RawAccessReaddetected(rule:RawAccessRead)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filecreated(rule:FileCreate)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileExecutableDetected(rule:FileExecutableDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filestreamcreated(rule:FileCreateStreamHash)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeleted(rule:FileDelete)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "FileDeletelogged(rule:FileDeleteDetected)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Filecreationtimechanged(rule:FileCreateTime)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Networkconnectiondetected(rule:NetworkConnect)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Dnsquery(rule:DnsQuery)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Driverloaded(rule:DriverLoad)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Imageloaded(rule:ImageLoad)", this.ProcessData);

            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryobjectaddedordeleted(rule:RegistryEvent)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryvalueset(rule:RegistryEvent)", this.ProcessData);
            this.traceEventSession.Source.Dynamic.AddCallbackForProviderEvent(this.sessionName, "Registryobjectrenamed(rule:RegistryEvent)", this.ProcessData);
        }

        /// <summary>
        /// Processes a system activity event by extracting relevant data and recording the action.
        /// </summary>
        /// <remarks>This method determines the type of system activity based on the event code and
        /// extracts the appropriate target information and process ID from the event data. It then records the action
        /// using the system activity recorder. If the event code is unknown or the process ID is not found, the method
        /// either returns without processing or throws an exception, respectively.</remarks>
        /// <param name="eventData">The event data containing information about the system activity to process.</param>
        /// <exception cref="SystemActivityException">Thrown if the <paramref name="eventData"/> does not contain a valid process ID.</exception>
        private void ProcessData(TraceEvent eventData)
        {
            string target;
            long processId;
            SysEventCode code;
            JObject syslog;

            // first, check if the eventData is null
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData), "Event data cannot be null.");
            }

            // Get the process ID from the event data
            if (eventData.PayloadByName("ProcessId") is long pid)
            {
                processId = pid;
            }
            else if (eventData.PayloadByName("ProcessId") is int pidInt)
            {
                processId = pidInt;
            }

            // when processAccessed event, the ProcessId is not present, but SourceProcessId is present.
            else if (eventData.PayloadByName("SourceProcessId") is long sourcePid)
            {
                processId = sourcePid;
            }
            else if (eventData.PayloadByName("SourceProcessId") is int sourcePidInt)
            {
                processId = sourcePidInt;
            }
            else
            {
                // Throw an exception if ProcessId is not found
                throw new SystemActivityException($"ProcessId not found in event data in {nameof(SysmonEventSession)}, target: {eventData}");
            }

            // check if the eventData is from Tracked Process
            if (!this.sysActRecorder.IsProcessTracked(processId))
            {
                // If the event is not from a tracked process, do nothing
                return;
            }

            code = SysmonEventDecoder.GetEventCodeFromData(eventData);

            switch (code)
            {
                case SysEventCode.ProcessCreation:
                case SysEventCode.ProcessTampering:
                    target = eventData.PayloadByName("Image")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.ProcessTerminated:
                    return;
                case SysEventCode.ProcessAccess:
                case SysEventCode.CreateRemoteThread:
                    target = eventData.PayloadByName("TargetImage")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.ImageLoad:
                case SysEventCode.DriverLoad:
                    target = eventData.PayloadByName("ImageLoaded")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.NetworkConnection:
                    target = eventData.PayloadByName("DestinationIp")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.DnsQuery:
                    target = eventData.PayloadByName("QueryName")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RegistryAdd:
                case SysEventCode.RegistrySet:
                case SysEventCode.RegistryDelete:
                    target = eventData.PayloadByName("TargetObject")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RegistryRename:
                    target = eventData.PayloadByName("NewName")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.FileCreate:
                case SysEventCode.FileDelete:
                case SysEventCode.FileModified:
                case SysEventCode.CreateStreamHash:
                    target = eventData.PayloadByName("TargetFilename")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.RawAccessReadDetected:
                    target = eventData.PayloadByName("Device")?.ToString() ?? string.Empty;
                    break;
                case SysEventCode.Unknown:
                default:
                    // Unknown event code, do nothing
                    return;
            }

            // decode the eventData to JObject
            syslog = this.sysmonTracepoint.GetSysmonLog(eventData);
            if (syslog == null)
            {
                // If syslog is null, it means the log is filtered. so do nothing
                return;
            }

            try
            {
                this.sysActRecorder.RecordProcessAction(processId, target, code, syslog);
            }
            catch (SystemActivityException ex)
            {
                // Log the exception if the process ID is not found
                Console.WriteLine($"Error processing Sysmon event data: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other exceptions that occur during processing
                throw new NanoException.BeaconException($"An error occurred while processing Sysmon event data: {ex.Message}", ex);
            }
        }
    }
}