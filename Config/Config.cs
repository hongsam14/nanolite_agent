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

        private readonly ConfigWrapper _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="configPath">configPath is the path of config.yml file.</param>
        /// <exception cref="ConfigException">raised when config file is uninterpretable.</exception>
        /// <exception cref="FileNotFoundException">raised when config file is not found.</exception>"
        public Config(string configPath)
        {
            string configStr;

            // Read the config file
            try
            {
                configStr = File.ReadAllText(configPath);
                if (configStr == null)
                {
                    throw new ConfigException("Config file is empty");
                }
            }
            catch (FileNotFoundException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ConfigException("Error while read configFile", e);
            }

            // Deserialize the config file
            IDeserializer deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();
            try
            {
                this._config = deserializer.Deserialize<ConfigWrapper>(configStr);
            }
            catch (Exception e)
            {
                throw new ConfigException("Error while deserializing config file", e);
            }

            // Set the properties from wrapper
            this.CollectorIP = this._config?.CollectorIP;
            if (this.CollectorIP == null)
            {
                throw new ConfigException("CollectorIP is not set in the config file");
            }

            this.CollectorPort = this._config?.CollectorPort;
            if (this.CollectorPort == null)
            {
                throw new ConfigException("CollectorPort is not set in the config file");
            }

            this.Exporter = this._config?.Exporter;
            if (this.Exporter == null)
            {
                throw new ConfigException("Exporter is not set in the config file");
            }
        }
    }
}