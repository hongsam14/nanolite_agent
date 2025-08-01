// <copyright file="FileDeleteMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a file deletion operation.
    /// </summary>
    /// <remarks>This class provides information about the file deletion process, including the image name,
    /// process ID, target filename, and user responsible for the operation. It is intended to be used in scenarios
    /// where tracking or auditing of file deletions is required.</remarks>
    public sealed class FileDeleteMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target filename that was deleted.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the file deletion operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
