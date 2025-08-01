// <copyright file="SystemActivityException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.NanoException
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when a system activity error occurs.
    /// </summary>
    [Serializable]
    public class SystemActivityException : BeaconException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityException"/> class.
        /// </summary>
        public SystemActivityException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SystemActivityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SystemActivityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActivityException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected SystemActivityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
