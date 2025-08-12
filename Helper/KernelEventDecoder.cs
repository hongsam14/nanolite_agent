// <copyright file="KernelEventDecoder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using System;
    using System.Diagnostics;
    using Microsoft.Diagnostics.Tracing;
    using Nanolite_agent.EventModel;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides methods for decoding kernel event trace data into structured JSON objects.
    /// </summary>
    /// <remarks>This class includes static methods for decoding specific types of kernel events, such as
    /// process creation and process termination events, from trace event data. The decoded events are returned as JSON
    /// objects for further processing or analysis. <para> The methods in this class are designed to handle trace events
    /// that conform to the expected structure for kernel events. If the input trace event does not match the expected
    /// structure, the behavior of the decoding methods may be undefined. </para></remarks>
    public static class KernelEventDecoder
    {
        /// <summary>
        /// Decodes a kernel process creation event from the specified trace event.
        /// </summary>
        /// <param name="origin">The trace event to decode, representing a process creation event.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded process creation event.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        public static JObject DecodeKernelProcessCreateEvent(TraceEvent origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            return EventFromCode(SysEventCode.ProcessCreation, origin);
        }

        /// <summary>
        /// Decodes a kernel process stop event from the specified trace event.
        /// </summary>
        /// <param name="origin">The trace event to decode, representing a process termination.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded process stop event.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        public static JObject DecodeKernelProcessStopEvent(TraceEvent origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            return EventFromCode(SysEventCode.ProcessTerminated, origin);
        }

        /// <summary>
        /// Decodes a kernel thread start event from the specified trace event.
        /// </summary>
        /// <param name="origin">The trace event to decode, representing a thread start event.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded thread start event.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        public static JObject DecodeKernelThreadStartEvent(TraceEvent origin)
        {
            JObject result;

            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            return EventFromCode(SysEventCode.ThreadStart, origin);
        }

        /// <summary>
        /// Decodes a kernel registry query event from the provided trace event.
        /// </summary>
        /// <param name="origin">The trace event containing the kernel registry query data. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the decoded kernel registry query event.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        public static JObject DecodeKernelRegistryQueryEvent(TraceEvent origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            return EventFromCode(SysEventCode.RegistryQuery, origin);
        }

        private static JObject EventFromCode(SysEventCode eventCode, TraceEvent origin)
        {
            JObject metadataObj, eventObj;

            eventObj = new JObject();

            // add common header
            eventObj.Add("EventID", (int)eventCode);

            eventObj.Add("EventName", value: Helper.SysEventCodeExtension.ToFriendlyString(eventCode));

            eventObj.Add("Source", "kernel");

            eventObj.Add("Timestamp", origin.TimeStamp);

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    metadataObj = KernelEventDecoder.DecodeKernelMetadata<KernelProcessCreationMetadata>(origin);
                    break;
                case SysEventCode.ProcessTerminated:
                    metadataObj = KernelEventDecoder.DecodeKernelMetadata<KernelProcessStopMetadata>(origin);
                    break;
                case SysEventCode.ThreadStart:
                    string imageName;

                    // get image name with process id.
                    try
                    {
                        imageName = Process.GetProcessById(origin.ProcessID).MainModule.FileName;
                    }
                    catch (Exception ex)
                    {
                        // log the exception
                        imageName = "unknown"; // fallback to unknown if unable to retrieve
                    }

                    metadataObj = KernelEventDecoder.DecodeKernelMetadata<KernelThreadStartMetadata>(origin);
                    metadataObj.Add("ImageFileName", imageName);
                    break;
                case SysEventCode.RegistryQuery:
                    // Use specific method for registry query metadata.
                    // That's because there is no pid in the PayloadNames of the TraceEvent
                    // and we need to set it manually.
                    metadataObj = KernelEventDecoder.DecodeKernelRegistryQueryMetadata(origin);
                    break;
                default:
                    return null;
            }

            eventObj.Add("Metadata", metadataObj);
            return eventObj;
        }

        private static JObject DecodeKernelMetadata<T>(TraceEvent origin)
            where T : IMetadata
        {
            JObject originObj;
            T convertedObj;

            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            // make new Json Object from TraceEvent
            originObj = new JObject();

            // copy all payloads to JObject
            for (int i = 0; i < origin.PayloadNames.Length; i++)
            {
                originObj.Add(origin.PayloadNames[i], JToken.FromObject(origin.PayloadValue(i)));
            }

            // convert originObj to Metadata Object to filter unrelated value
            convertedObj = originObj.ToObject<T>();

            // convert T to JObject
            return JObject.FromObject(convertedObj);
        }

        /// <summary>
        /// Converts a <see cref="TraceEvent"/> instance into a JSON object containing kernel registry query metadata.
        /// </summary>
        /// <remarks>This method processes the payload of the provided <see cref="TraceEvent"/> to extract
        /// relevant metadata, filters unrelated values, and includes the process ID from the original event.</remarks>
        /// <param name="origin">The <see cref="TraceEvent"/> instance to be converted. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="JObject"/> representing the kernel registry query metadata extracted from the provided
        /// <paramref name="origin"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="origin"/> is <see langword="null"/>.</exception>
        private static JObject DecodeKernelRegistryQueryMetadata(TraceEvent origin)
        {
            JObject originObj;
            KernelRegistryQueryMetadata convertedObj;

            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            // make new Json Object from TraceEvent
            originObj = new JObject();

            // copy all payloads to JObject
            for (int i = 0; i < origin.PayloadNames.Length; i++)
            {
                originObj.Add(origin.PayloadNames[i], JToken.FromObject(origin.PayloadValue(i)));
            }

            // convert originObj to Metadata Object to filter unrelated value
            convertedObj = originObj.ToObject<KernelRegistryQueryMetadata>();

            // set pid manually
            convertedObj.ProcessID = origin.ProcessID;

            // convert T to JObject
            return JObject.FromObject(convertedObj);
        }
    }
}
