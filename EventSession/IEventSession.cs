// <copyright file="IEventSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.EventSession
{
    /// <summary>
    /// IEventSession interface defines the methods to start, stop and wait of the common etw session.
    /// </summary>
    public interface IEventSession
    {
        /// <summary>
        /// StartSession method is used to start the etw session.
        /// </summary>
        void StartSession();

        /// <summary>
        /// StopSession method is used to stop the etw session.
        /// </summary>
        void StopSession();

        /// <summary>
        /// WaitSession method is used to wait the etw session.
        /// </summary>
        void WaitSession();
    }
}
