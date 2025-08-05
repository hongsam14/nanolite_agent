// <copyright file="FileCreateMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents file create metadata associated with a file event, including process information and file details.
    /// </summary>
    /// <remarks>This class provides properties to access various details about a file event, such as the
    /// process ID, the image name, the target filename, the creation time in UTC, and the user associated with the
    /// event.</remarks>
    public sealed class FileCreateMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID associated with the file event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the file event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the target filename involved in the file event.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the file event in UTC.
        /// </summary>
        [JsonProperty(nameof(CreationUtcTime))]
        public string CreationUtcTime { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the file event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
