// <copyright file="NetworkConnectionMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata for a network connection, providing details about the source and destination endpoints, the
    /// protocol used, and the process initiating the connection.
    /// </summary>
    /// <remarks>This class is used to encapsulate information about a network connection, including both
    /// source and destination details such as IP addresses, hostnames, and ports. It also includes metadata about the
    /// process that initiated the connection, such as the process ID and GUID, as well as user and image information.
    /// This metadata can be useful for logging, monitoring, or analyzing network activity.</remarks>
    public sealed class NetworkConnectionMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the destination host name for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationHostname))]
        public string DestinationHostname { get; set; }

        /// <summary>
        /// Gets or sets the destination IP address for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationIp))]
        public string DestinationIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination IP is IPv6.
        /// </summary>
        [JsonProperty(nameof(DestinationIsIpv6))]
        public bool DestinationIsIpv6 { get; set; }

        /// <summary>
        /// Gets or sets the destination port for the network connection.
        /// </summary>
        [JsonProperty(nameof(DestinationPort))]
        public long DestinationPort { get; set; }

        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection was initiated.
        /// </summary>
        [JsonProperty(nameof(Initiated))]
        public bool Initiated { get; set; }

        /// <summary>
        /// Gets or sets the process GUID associated with the network connection.
        /// </summary>
        [JsonProperty(nameof(ProcessGuid))]
        public string ProcessGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the protocol used for the network connection.
        /// </summary>
        [JsonProperty(nameof(Protocol))]
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the source host name for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourceHostname))]
        public string SourceHostname { get; set; }

        /// <summary>
        /// Gets or sets the source IP address for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourceIp))]
        public string SourceIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source IP is IPv6.
        /// </summary>
        [JsonProperty(nameof(SourceIsIpv6))]
        public bool SourceIsIpv6 { get; set; }

        /// <summary>
        /// Gets or sets the source port for the network connection.
        /// </summary>
        [JsonProperty(nameof(SourcePort))]
        public long SourcePort { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the current operation.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent image.
        /// </summary>
        [JsonProperty(nameof(ParentImage))]
        public string ParentImage { get; set; }
    }
}
