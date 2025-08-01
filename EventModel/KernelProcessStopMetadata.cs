// <copyright file="KernelProcessStopMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata related to the termination of a kernel process.
    /// </summary>
    /// <remarks>This class provides information about a kernel process that has stopped, including its unique
    /// identifier, the name of the image file associated with the process, and the exit code returned upon
    /// termination.</remarks>
    public sealed class KernelProcessStopMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the process.
        /// </summary>
        [JsonProperty(nameof(ProcessID))]
        public long ProcessID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the image name.
        /// </summary>
        [JsonProperty(nameof(ImageFileName))]
        public string ImageFileName { get; set; }

        /// <summary>
        /// Gets or sets the exit code of the process.
        /// </summary>
        [JsonProperty(nameof(ExitCode))]
        public int ExitCode { get; set; }
    }
}
