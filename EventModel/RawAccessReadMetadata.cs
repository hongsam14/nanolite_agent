// <copyright file="RawAccessReadMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a raw access read operation.
    /// </summary>
    /// <remarks>This class provides details about a raw access read operation, including the image name, 
    /// process ID, target device, and user information. It is primarily used to encapsulate  information for logging,
    /// auditing, or analysis purposes.</remarks>
    public sealed class RawAccessReadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the details of the raw access read operation.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the raw access read operation.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the target object of the raw access read operation.
        /// </summary>
        [JsonProperty(nameof(Device))]
        public string Device { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
