﻿// <copyright file="BeaconException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.NanoException
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents errors that occur during beacon operations.
    /// </summary>
    [Serializable]
    public class BeaconException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeaconException"/> class.
        /// </summary>
        public BeaconException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeaconException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BeaconException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeaconException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BeaconException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeaconException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected BeaconException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
