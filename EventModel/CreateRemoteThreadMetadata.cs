// <copyright file="CreateRemoteThreadMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with the creation of a remote thread in a target process.
    /// </summary>
    public sealed class CreateRemoteThreadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the process ID of the source process that created the remote thread.
        /// </summary>
        [JsonProperty(nameof(SourceProcessId))]
        public long SourceProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name of the source process that created the remote thread.
        /// </summary>
        [JsonProperty(nameof(SourceImage))]
        public string SourceImage { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the source process.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the target process where the remote thread was created.
        /// </summary>
        [JsonProperty(nameof(TargetProcessId))]
        public long TargetProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name of the target process where the remote thread was created.
        /// </summary>
        [JsonProperty(nameof(TargetImage))]
        public string TargetImage { get; set; }

        /// <summary>
        /// Gets or sets the module name where the remote thread was created.
        /// </summary>
        public string StartModule { get; set; }

        /// <summary>
        /// Gets or sets the function name where the remote thread was created.
        /// </summary>
        public string StartFunction { get; set; }
    }
}
