// <copyright file="RegistryAddMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a registry addition event.
    /// </summary>
    /// <remarks>This class provides information about a registry addition, including the process ID,  the
    /// image name, the target object, and the user responsible for the action.</remarks>
    public sealed class RegistryAddMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry addition operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry addition.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry addition.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry addition operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
