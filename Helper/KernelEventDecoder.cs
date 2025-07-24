// <copyright file="KernelEventDecoder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents metadata for the creation of a kernel process, event id == 1.
    /// </summary>
    public sealed class KernelProcessCreationMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessID))]
        public long ProcessID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentID))]
        public long ParentID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the image name.
        /// </summary>
        [JsonProperty(nameof(ImageFileName))]
        public string ImageFileName { get; set; }
    }

    public sealed class KernelProcessStopMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessID))]
        public long ProcessID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the image name.
        /// </summary>
        [JsonProperty(nameof(ImageFileName))]
        public string ImageFileName { get; set; }

        /// <summary>
        /// Gets or sets the exit code of the process.
        /// </summary>
        [JsonProperty(nameof(ExitCode))]
        public int ExitCode { get; set; }
    }

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

        private static JObject EventFromCode(SysEventCode eventCode, TraceEvent origin)
        {
            JObject metadataObj, eventObj;

            eventObj = new JObject();

            // add common header
            eventObj.Add("EventID", (int)eventCode);

            eventObj.Add("EventName", value: Helper.SysEventCodeExtension.ToFriendlyString(eventCode));

            eventObj.Add("Source", "kernel");

            eventObj.Add("TimeStamp", origin.TimeStamp);

            switch (eventCode)
            {
                case SysEventCode.ProcessCreation:
                    metadataObj = KernelEventDecoder.DecodeKernelMetadata<KernelProcessCreationMetadata>(origin);
                    break;
                case SysEventCode.ProcessTerminated:
                    metadataObj = KernelEventDecoder.DecodeKernelMetadata<KernelProcessStopMetadata>(origin);
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
    }
}
