// <copyright file="ProcessTerminatedMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a terminated process.
    /// </summary>
    /// <remarks>This class provides information about a process that has been terminated, including its ID,
    /// the image name, and the user who initiated it.</remarks>
    public sealed class ProcessTerminatedMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID associated with the DNS query event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the DNS query event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
