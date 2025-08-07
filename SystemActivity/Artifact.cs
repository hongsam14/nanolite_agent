// <copyright file="Artifact.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.SystemActivity
{
    using System;
    using nanolite_agent.Properties;

    /// <summary>
    /// Defines the type of artifact that an actor can interact with.
    /// </summary>
    public class Artifact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Artifact"/> class with the specified type and name.
        /// </summary>
        /// <param name="objectType">The type of the artifact, indicating its category or classification.</param>
        /// <param name="objectName">The name of the artifact. This value cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="objectName"/> is <see langword="null"/>
        /// or <paramref name="objectType"/> is <see cref="ArtifactType.Undefined"/>.
        /// </exception>
        public Artifact(ArtifactType objectType, string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || objectType == ArtifactType.UNDEFINED)
            {
                throw new ArgumentNullException(nameof(objectName), DebugMessages.SystemActivityNullException);
            }

            this.ObjectType = objectType;
            this.ObjectName = objectName;
        }

        /// <summary>
        /// Gets object type of the artifact.
        /// </summary>
        public ArtifactType ObjectType { get; private set; }

        /// <summary>
        /// Gets the name of the artifact.
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Gets an identifier for the artifact based on its type and name.
        /// The format is "{objectName}@{objectType}".
        /// </summary>
        public string ArtifectID
        {
            get
            {
                // Return null or empty if objectName is null/empty or objectType is Undefined
                if (string.IsNullOrEmpty(this.ObjectName) || this.ObjectType == ArtifactType.UNDEFINED)
                {
                    return null;
                }

                // Generate a unique ID for the artifect based on its type and name
                return $"{this.ObjectName}@{this.ObjectType}";
            }
        }
    }
}
