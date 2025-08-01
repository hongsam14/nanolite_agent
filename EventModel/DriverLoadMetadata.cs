// <copyright file="DriverLoadMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a driver load, including hash and signature information.
    /// </summary>
    /// <remarks>This class provides properties to access the hash values and signature details of a driver,
    /// which can be used for validation and verification purposes.</remarks>
    public sealed class DriverLoadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the image loaded associated with the object.
        /// </summary>
        [JsonProperty(nameof(ImageLoaded))]
        public string ImageLoaded { get; set; }

        /// <summary>
        /// Gets or sets the hash values associated with the object.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the digital signature associated with the object.
        /// </summary>
        [JsonProperty(nameof(Signature))]
        public string Signature { get; set; }
    }
}
