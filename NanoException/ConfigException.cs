// <copyright file="ConfigException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.NanoException
{
    using System;

    /// <summary>
    /// ConfigException Exception class of the Nanolite agent.
    /// It is raised when config file is uninterpretable.
    /// </summary>
    [Serializable]
    public sealed class ConfigException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        public ConfigException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        public ConfigException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        /// <param name="inner">origin of the exception.</param>
        public ConfigException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="serializationInfo">stores all data needed to serialize & deserialize.</param>
        /// <param name="streamingContext">describe source and destination of serialized stream.</param>
        private ConfigException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
