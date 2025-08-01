// <copyright file="DnsQueryMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with a DNS query, including details about the process and user that initiated the
    /// query.
    /// </summary>
    /// <remarks>This class provides properties to access information such as the image name of the process,
    /// the process ID, the DNS query name, and the user who initiated the query.</remarks>
    public sealed class DnsQueryMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the URL of the image associated with this entity.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the DNS query name.
        /// </summary>
        [JsonProperty(nameof(QueryName))]
        public string QueryName { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the current operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

    }

}
