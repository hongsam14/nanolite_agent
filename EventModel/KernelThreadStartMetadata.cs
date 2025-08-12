// <copyright file="KernelThreadStartMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata for the start of a kernel thread, event id == 3.
    /// </summary>
    public sealed class KernelThreadStartMetadata: IMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ParentProcessID))]
        public long ParentProcessID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentThreadID))]
        public long ParentThreadID { get; set; }
    }
}
