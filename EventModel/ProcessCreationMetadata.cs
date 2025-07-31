// <copyright file="ProcessCreationMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents metadata associated with the creation of a process.
    /// </summary>
    /// <remarks>This class provides detailed information about a process, including its command line, 
    /// associated company, current directory, file version, and more. It also includes metadata  about the parent
    /// process and user context. This information can be useful for auditing,  monitoring, or analyzing process
    /// creation events.</remarks>
    public sealed class ProcessCreationMetadata : IMetadata
    {
        /// <summary>
        /// Gets or sets the command line used to start the process.
        /// </summary>
        [JsonProperty(nameof(CommandLine))]
        public string CommandLine { get; set; }

        /// <summary>
        /// Gets or sets the company associated with the process image.
        /// </summary>
        [JsonProperty(nameof(Company))]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the current directory of the process.
        /// </summary>
        [JsonProperty(nameof(CurrentDirectory))]
        public string CurrentDirectory { get; set; }

        /// <summary>
        /// Gets or sets the file version of the process image.
        /// </summary>
        [JsonProperty(nameof(FileVersion))]
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the hash values of the process image.
        /// </summary>
        [JsonProperty(nameof(Hashes))]
        public string Hashes { get; set; }

        /// <summary>
        /// Gets or sets the image name.
        /// </summary>
        [JsonProperty(nameof(Image))]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the integrity level of the process.
        /// </summary>
        [JsonProperty(nameof(IntegrityLevel))]
        public string IntegrityLevel { get; set; }

        /// <summary>
        /// Gets or sets the logon ID associated with the process.
        /// </summary>
        [JsonProperty(nameof(LogonId))]
        public string LogonId { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the process image.
        /// </summary>
        [JsonProperty(nameof(OriginalFileName))]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the command line of the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentCommandLine))]
        public string ParentCommandLine { get; set; }

        /// <summary>
        /// Gets or sets the parent image name associated with the network connection.
        /// </summary>
        [JsonProperty(nameof(ParentImage))]
        public string ParentImage { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the parent process.
        /// </summary>
        [JsonProperty(nameof(ParentProcessId))]
        public long ParentProcessId { get; set; }

        /// <summary>
        /// Gets or sets the process ID.
        /// </summary>
        [JsonProperty(nameof(ProcessId))]
        public long ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the product name associated with the process image.
        /// </summary>
        [JsonProperty(nameof(Product))]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonProperty(nameof(User))]
        public string User { get; set; }
    }
}
