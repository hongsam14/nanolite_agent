// <copyright file="RegistryDeleteMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a registry delete operation.
    /// </summary>
    /// <remarks>This class provides details about a registry delete operation, including the image involved,
    /// the process ID, and the target object of the operation. It is used to encapsulate information relevant to the
    /// deletion event.</remarks>
    public sealed class RegistryDeleteMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry delete operation.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }
    }
}
