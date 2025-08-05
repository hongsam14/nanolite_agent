// <copyright file="RegistrySetMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a registry set operation.
    /// </summary>
    /// <remarks>This class provides details about a registry set operation, including the user who performed
    /// the operation, the target object affected, and additional descriptive information.</remarks>
    public sealed class RegistrySetMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry set operation.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry set operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry set operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry set operation.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry set operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
