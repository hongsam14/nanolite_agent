// <copyright file="KernelRegistryQueryMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata for a kernel registry query event.
    /// </summary>
    public sealed class KernelRegistryQueryMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessID))]
        public long ProcessID { get; set; }

        /// <summary>
        /// Gets or sets registry Key name that was queried.
        /// </summary>
        [JsonProperty(nameof(KeyName))]
        public string KeyName { get; set; }
    }
}
