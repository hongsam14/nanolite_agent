// <copyright file="Artifect.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity
{
    using System;
    using nanolite_agent.Properties;

    /// <summary>
    /// Defines the type of artifect that an actor can interact with.
    /// </summary>
    public class Artifect
    {
        private readonly string objectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Artifect"/> class with the specified type and name.
        /// </summary>
        /// <param name="objectType">The type of the artifact, indicating its category or classification.</param>
        /// <param name="objectName">The name of the artifact. This value cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="objectName"/> is <see langword="null"/>
        /// or <paramef name="objectType"/> is <see cref="ArtifectType.Undefined"/>.
        /// </exception>
        public Artifect(ArtifectType objectType, string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || objectType == ArtifectType.Undefined)
            {
                throw new ArgumentNullException(nameof(objectName), DebugMessages.SystemActivityNullException);
            }

            this.ObjectType = objectType;
            this.objectName = objectName;
        }

        /// <summary>
        /// Gets Object type of the artifect.
        /// </summary>
        public ArtifectType ObjectType { get; private set; }

        /// <summary>
        /// Gets provides a identifier for the artifect based on its type and name.
        /// The format is "{objectName}@{objectType}".
        /// </summary>
        public string ArtifectID
        {
            get
            {
                // Return null or empty if objectName is null/empty or objectType is Undefined
                if (string.IsNullOrEmpty(this.objectName) || this.ObjectType == ArtifectType.Undefined)
                {
                    return null;
                }

                // Generate a unique ID for the artifect based on its type and name
                return $"{this.objectName}@{this.ObjectType}";
            }
        }
    }
}
