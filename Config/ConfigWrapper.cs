// <copyright file="ConfigWrapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Config
{
    /// <summary>
    /// ConfigWrapper is a wrapper class for the config file.
    /// </summary>
    public class ConfigWrapper
    {
        /// <summary>
        /// Gets or sets the IP address of the collector.
        /// </summary>
        public string CollectorIP { get; set; }

        /// <summary>
        /// Gets or sets the port of the collector.
        /// </summary>
        public string CollectorPort { get; set; }

        /// <summary>
        /// Gets or sets the exporter type.
        /// </summary>
        public string Exporter { get; set; }
    }
}
