// <copyright file="CreateStreamHashMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata for creating a stream hash, including details such as creation time, hash value, and
    /// associated user.
    /// </summary>
    /// <remarks>This class is used to encapsulate metadata information required for the creation of a stream
    /// hash. It includes properties for the creation time in UTC, the hash value, the image associated with the hash,
    /// the process ID, the target filename, and the user who initiated the creation.</remarks>
    public sealed class CreateStreamHashMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the creation time of the stream hash in UTC.
        /// </summary>
        [JsonProperty(nameof(CreationUtcTime))]
        public string CreationUtcTime { get; set; }

        /// <summary>
        /// Gets or sets the hash value of the stream.
        /// </summary>
        [JsonProperty(nameof(Hash))]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the image associated with the stream hash.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the stream hash creation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target filename for which the stream hash was created.
        /// </summary>
        [JsonProperty(nameof(TargetFilename))]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the stream hash creation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
