// <copyright file="ConfigWrapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Config
{
    /// <summary>
    /// ConfigWrapper is a wrapper class for the config file.
    /// </summary>
    internal class ConfigWrapper
    {
        public string CollectorIP { get; set; }

        public string CollectorPort { get; set; }

        public string Exporter { get; set; }
    }
}
