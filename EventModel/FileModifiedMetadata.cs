// <copyright file="FileModifiedMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a file modification event.
    /// </summary>
    /// <remarks>This class provides detailed information about a file modification event, including the
    /// process and user responsible for the modification, the target file, and timestamps related to the file's
    /// creation. It is designed to be used in scenarios where tracking file changes is required, such as auditing or
    /// monitoring systems.</remarks>
    public sealed class FileModifiedMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID associated with the file modification event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the file modification event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the target filename that was modified.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the file modification operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the file in UTC.
        /// </summary>
        [JsonProperty(nameof(CreationUtcTime), NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreationUtcTime { get; set; }

        /// <summary>
        /// Gets or sets the previous creation time of the file in UTC.
        /// </summary>
        [JsonProperty(nameof(PreviousCreationUtcTime), NullValueHandling = NullValueHandling.Ignore)]
        public DateTime PreviousCreationUtcTime { get; set; }
    }

}
