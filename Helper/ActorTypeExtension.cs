// <copyright file="ActorTypeExtension.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Helper
{
    using System.Diagnostics;
    using Nanolite_agent.SystemActivity;
    using Nanolite_agent.NanoException;

    /// <summary>
    /// ActorTypeExtensions class provides extension methods for ActorType enum.
    /// </summary>
    public static class ActorTypeExtension
    {
        /// <summary>
        /// Determines the activity type of the specified actor based on its characteristics.
        /// ActorActivityType is very closely related to the direction of the Graph.
        /// - READ_RECV actors are those that receive data or read from a source.
        /// - WRITE_SEND actors are those that send data or write to a destination.
        /// </summary>
        /// <param name="type">The actor type to evaluate.</param>
        /// <returns>An <see cref="ActorActivityType"/> indicating the activity type of the actor. Returns <see
        /// cref="ActorActivityType.READ_RECV"/> if the actor is a read/receive actor, or <see
        /// cref="ActorActivityType.WRITE_SEND"/> if it is a write/send actor.</returns>
        /// <exception cref="SystemActivityException">Thrown if the specified <paramref name="type"/> is not a supported actor type.</exception>
        public static ActorActivityType GetActorActivityTypeFromActorType(this ActorType type)
        {
            if (type.IsReadRecvActor())
            {
                return ActorActivityType.READ_RECV;
            }
            else if (type.IsWriteSendActor())
            {
                return ActorActivityType.WRITE_SEND;
            }
            else
            {
                return ActorActivityType.NOT_ACTOR;
            }
        }

        /// <summary>
        /// Sets the activity type tag on the specified activity based on the provided actor activity type.
        /// </summary>
        /// <param name="dest">The activity on which to set the type tag.</param>
        /// <param name="destType">The type of actor activity to be set. Must be either <see cref="ActorActivityType.READ_RECV"/> or <see
        /// cref="ActorActivityType.WRITE_SEND"/>.</param>
        /// <exception cref="SystemActivityException">Thrown if <paramref name="destType"/> is not a supported <see cref="ActorActivityType"/>.</exception>
        public static void TagActorActivityAttribute(Activity dest, ActorActivityType destType)
        {
            switch (destType)
            {
                case ActorActivityType.READ_RECV:
                    dest.SetTag("act.type", "read/recv");
                    break;
                case ActorActivityType.WRITE_SEND:
                    dest.SetTag("act.type", "write/send");
                    break;
                default:
                    // raise an exception for unsupported types
                    throw new SystemActivityException(
                        $"Unsupported ActorActivityType: {destType}. " +
                        "Please use READ_RECV or WRITE_SEND.");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="ActorType"/> is a getter actor.
        /// </summary>
        /// <param name="type">The <see cref="ActorType"/> to evaluate.</param>
        /// <returns><see langword="true"/> if the specified <paramref name="type"/> is a read/recv actor; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool IsReadRecvActor(this ActorType type)
        {
            switch (type)
            {
                case ActorType.REMOTE_THREAD:
                case ActorType.ACCEPT:
                case ActorType.LOAD:
                case ActorType.RAW_ACCESS_READ_DETECTED:
                case ActorType.REG_QUERY:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="ActorType"/> is a setter actor.
        /// </summary>
        /// <param name="type">The <see cref="ActorType"/> to evaluate. </param>
        /// <returns><see langword="true"/> if the specified <paramref name="type"/> is a write/send actor; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool IsWriteSendActor(this ActorType type)
        {
            switch (type)
            {
                case ActorType.TAMPERING:
                case ActorType.ACCESS:
                case ActorType.CONNECT:
                case ActorType.CREATE:
                case ActorType.DELETE:
                case ActorType.MODIFY:
                case ActorType.CREATE_STREAM_HASH:
                case ActorType.REG_ADD:
                case ActorType.REG_DELETE:
                case ActorType.REG_SET:
                case ActorType.REG_RENAME:
                    return true;
                default:
                    return false;
            }
        }
    }
}
