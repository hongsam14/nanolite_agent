// <copyright file="EventBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the base class for a Sysmon event, providing common properties for event data.
    /// </summary>
    /// <remarks>This class serves as a foundation for specific Sysmon event types, encapsulating shared
    /// properties such as timestamp, event code, event name, and source. Derived classes should implement the  <see
    /// cref="MetadataObject"/> property to provide event-specific metadata.</remarks>
    public abstract class EventBase
    {
        /// <summary>
        /// Gets or sets the timestamp of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(Timestamp))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the event code associated with the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(EventCode))]
        public int EventCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(EventName))]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the source of the Sysmon event.
        /// </summary>
        [JsonProperty(nameof(Source))]
        public string Source { get; set; }

        /// <summary>
        /// Gets the metadata object associated with the current sys event.
        /// </summary>
        [JsonIgnore]
        public abstract IMetadata MetadataObject { get; }
    }
}
