// <copyright file="ImageLoadMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with an image load event in a process.
    /// </summary>
    /// <remarks>This class provides properties to access various details about the image load, such as the
    /// company name, file version, and process ID. It is typically used in scenarios where detailed information about
    /// loaded images is required for logging, auditing, or analysis purposes.</remarks>
    public sealed class ImageLoadMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the name of the company associated with the entity.
        /// </summary>
        [JsonProperty(nameof(Company))]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        [JsonProperty(nameof(FileVersion))]
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the hash values associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the event.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the image.
        /// </summary>
        [JsonProperty(nameof(OriginalFileName))]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the process ID associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the product name associated with the image.
        /// </summary>
        [JsonProperty(nameof(Product))]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the image load event.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the image loaded associated with the object.
        /// </summary>
        [JsonProperty(nameof(ImageLoaded))]
        public string ImageLoaded { get; set; }
    }
}
