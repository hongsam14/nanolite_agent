// <copyright file="ProcessAccessMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata related to process access events, including information about the source and target
    /// processes and users.
    /// </summary>
    /// <remarks>This class provides properties to access details about the source and target processes
    /// involved in an access event, such as their image names, process IDs, and associated users.</remarks>
    public sealed class ProcessAccessMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the image name of the source process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(SourceImage))]
        public string SourceImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the source process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(SourceProcessId))]
        public long SourceProcessId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the source process.
        /// </summary>
        [JsonProperty(nameof(SourceUser))]
        public string SourceUser { get; set; }

        /// <summary>
        /// Gets or sets the image name of the target process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(TargetImage))]
        public string TargetImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the target process involved in the access event.
        /// </summary>
        [JsonProperty(nameof(TargetProcessId))]
        public long TargetProcessId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the target process.
        /// </summary>
        [JsonProperty(nameof(TargetUser))]
        public string TargetUser { get; set; }
    }
}
