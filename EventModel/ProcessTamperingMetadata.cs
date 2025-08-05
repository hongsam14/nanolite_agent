// <copyright file="ProcessTamperingMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata related to process tampering events, including details about the user, process, and
    /// tampering type.
    /// </summary>
    /// <remarks>This class provides information about a process tampering event, such as the user associated
    /// with the process,  the process ID, the image name, and the type of tampering that occurred. It is typically used
    /// to capture and  analyze metadata for security or diagnostic purposes.</remarks>
    public sealed class ProcessTamperingMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the user associated with the source process.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

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
        /// Gets or sets the type of tampering that occurred.
        /// </summary>
        public string Type { get; set; }
    }
}
