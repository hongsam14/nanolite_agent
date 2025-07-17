// <copyright file="Config.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Config
{
    using System;
    using System.IO;
    using Nanolite_agent.NanoException;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// Config class of the Nanolite agent.
    /// config file format must be in yaml.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="configWrapper">configWrapper is the wrapper of the config properties.</param>
        /// <exception cref="ConfigException">raised when config file is uninterpretable.</exception>
        /// <exception cref="FileNotFoundException">raised when config file is not found.</exception>"
        public Config(ConfigWrapper configWrapper)
        {
            // Set the properties from wrapper
            this.CollectorIP = configWrapper?.CollectorIP;
            if (this.CollectorIP == null || this.CollectorIP.Length == 0)
            {
                throw new ConfigException("CollectorIP is not set in the config file");
            }

            this.CollectorPort = configWrapper?.CollectorPort;
            if (this.CollectorPort == null || this.CollectorPort.Length == 0)
            {
                throw new ConfigException("CollectorPort is not set in the config file");
            }

            this.Exporter = configWrapper?.Exporter;
            if (this.Exporter == null || this.Exporter.Length == 0)
            {
                throw new ConfigException("Exporter is not set in the config file");
            }
        }

        /// <summary>
        /// Gets CollectorIP property of the <see cref="Config"/> class.
        /// CollectorIP is the IP address of the collector framework like Jaeger, Zipkin etc...
        /// </summary>
        public string CollectorIP { get; private set; }

        /// <summary>
        /// Gets CollectorPort property of the <see cref="Config"/> class.
        /// CollectorPort is the port number of the collector framework like Jaeger, Zipkin etc...
        public string CollectorPort { get; private set; }

        /// <summary>
        /// Gets Exporter property of the <see cref="Config"/> class.
        /// Exporter is the exporter type of the collector framework like Jaeger, Zipkin, OTel etc...
        /// </summary>
        public string Exporter { get; private set; }

    }
}