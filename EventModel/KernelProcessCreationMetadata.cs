// <copyright file="KernelProcessCreationMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

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
}
