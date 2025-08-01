// <copyright file="RegistryRenameMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a registry rename event.
    /// </summary>
    /// <remarks>This class provides details about a registry rename event, including information about the process
    /// and user involved.</remarks>
    public sealed class RegistryRenameMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the registry event.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the registry event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the registry event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the registry event.
        /// </summary>
        [JsonProperty(nameof(TargetObject))]
        public string TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the registry event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the new name of the registry key after the rename operation.
        /// </summary>
        [JsonProperty(nameof(NewName))]
        public string NewName { get; set; }
    }
}
