// <copyright file="ConfigExtension.cs" company="PlaceholderCompany">
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
    /// Provides extension methods for loading configuration data from files.
    /// </summary>
    /// <remarks>This static class contains methods that extend the functionality of strings to facilitate the
    /// loading and deserialization of configuration data from file paths.</remarks>
    public static class ConfigExtension
    {
        /// <summary>
        /// Loads a configuration from the specified file path and returns a <see cref="Config"/> object.
        /// </summary>
        /// <param name="configPath">The file path to the configuration file. Must not be null or empty.</param>
        /// <returns>A <see cref="Config"/> object representing the configuration data.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="configPath"/> is null or empty.</exception>
        /// <exception cref="ConfigException">Thrown if there is an error reading the file, if the file is empty, or if there is an error during
        /// deserialization.</exception>
        public static Config LoadConfigFromPath(this string configPath)
        {
            string configStr;
            ConfigWrapper configWrapper;

            // check argument is not null or empty
            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ArgumentException("Config path cannot be null or empty", nameof(configPath));
            }

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
                configWrapper = deserializer.Deserialize<ConfigWrapper>(configStr);
            }
            catch (Exception e)
            {
                throw new ConfigException("Error while deserializing config file", e);
            }

            return new Config(configWrapper);
        }
    }
}
