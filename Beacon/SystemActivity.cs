using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanolite_agent.Beacon
{
    /// <summary>
    /// ArtifectType enum represents Actor's artifect type.
    /// </summary>
    public enum ArtifectType
    {
        /// <summary>
        /// File system artifect.
        /// </summary>
        File,

        /// <summary>
        /// Registry artifect.
        /// </summary>
        Registry,

        /// <summary>
        /// Network artifect.
        /// </summary>
        Network,

        /// <summary>
        ///  Process artifect. This is not Process Object. This is a type of artifect that represents a process injection.
        /// </summary>
        Process,

        /// <summary>
        /// Module artifect
        /// </summary>
        Module,
    }

    /// <summary>
    /// ActorType enum represents the type of actor in the system.
    /// </summary>
    public enum ActorType
    {
        /************************************************************************
        * Process injection related actors
        * ***********************************************************************/

        /// <summary>
        /// Remote thread actor. This actor represents sysmon-event-8
        /// </summary>
        REMOTE_THREAD,

        /// <summary>
        /// Tampering actor. This actor represents sysmon-event-25
        /// </summary>
        TAMPERING,

        /************************************************************************
         * Network related actors
         * **********************************************************************/

        /// <summary>
        /// Connect actor. This actor represents sysmon-event-3
        /// </summary>
        CONNECT,

        /// <summary>
        /// Accept actor. This actor is not related to sysmon-event. This actor represents a network connection that is accepted by the system.
        /// </summary>
        ACCEPT,

        /************************************************************************
         * File system related actors
         * **********************************************************************/

        /// <summary>
        /// Create actor. This actor represents file-creation events such as sysmon-event-11, 29.
        /// </summary>
        CREATE,

        /// <summary>
        /// Delete actor. This actor represents file-deletion events such as sysmon-event-23.
        /// </summary>
        DELETE,

        /// <summary>
        /// Modify actor. This actor represents file-modification events such as sysmon-event-2
        /// </summary>
        MODIFY,

        /// <summary>
        /// Create stream hash actor. This actor represents sysmon-event-15.
        /// </summary>
        CREATE_STREAM_HASH,

        /* **********************************************************************
         * Registry related actors
         * **********************************************************************/

        /// <summary>
        /// Registry add actor. This actor represents sysmon-event-12.
        /// </summary>
        REG_ADD,

        /// <summary>
        /// Registry delete actor. This actor represents sysmon-event-12.
        /// </summary>
        REG_DELETE,

        /// <summary>
        /// Registry set actor. This actor represents sysmon-event-13.
        /// </summary>
        REG_SET,

        /// <summary>
        /// Registry rename actor. This actor represents sysmon-event-14.
        /// </summary>
        REG_RENAME,
    }

    public static class ActorTypeExtensions
    {
        public static bool IsGetter(this ActorType type)
        {
            switch (type)
            {
                case ActorType.REMOTE_THREAD:
                case ActorType.ACCEPT:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSetter(this ActorType type)
        {
            switch (type)
            {
                case ActorType.TAMPERING:
                case ActorType.CONNECT:
                case ActorType.CREATE:
                case ActorType.RENAME:
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

    public class Artifect
    {
        public ArtifectType ObjectType { get; private set; }

        public string ObjectName { get; private set; }

        public string ObjectID
        {
            get
            {
                if (string.IsNullOrEmpty(this.ObjectName))
                {
                    throw new NanoException.BeaconException("ObjectName cannot be null or empty");
                }

                // Generate a unique ID for the artifect based on its type and name
                return $"{this.ObjectName}@{this.ObjectType.ToString()}";
            }
        }

        public Artifect(ArtifectType objectType, string objectName)
        {
            this.ObjectType = objectType;
            this.ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName), "ObjectName cannot be null");
        }
    }

    public class ActorContext
    {
        public ActorType type { get; private set; }

        public Artifect ArtifectObject { get; private set; }

        public int LogCount { get; private set; }

        public string ActorID
        {
            get
            {
                if (this.ArtifectObject == null)
                {
                    throw new NanoException.BeaconException("ArtifectObject cannot be null");
                }

                // Generate a unique ID for the actor based on its type and artifect object ID
                return $"{this.ArtifectObject.ObjectID}@{this.type.ToString()}";
            }
        }

        public ActorContext(Artifect artifectObject, ActorType type)
        {
            if (artifectObject == null)
            {
                throw new ArgumentNullException(nameof(artifectObject), "Artifect object cannot be null");
            }

            // set artifect object
            this.ArtifectObject = artifectObject;

            // set actor type
            this.type = type;

            // Initialize LogCount to 0
            this.LogCount = 0;
        }

        public void IncrementLogCount()
        {
            this.LogCount++;
        }
    }

    public class ActorActivityContext
    {
        public Activity Activity { get; private set; }
        public ActorContext Actor { get; private set; }
        public ActorActivityContext(Activity activity, ActorContext actor)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity), "Activity cannot be null");
            }
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor), "Actor cannot be null");
            }
            this.Activity = activity;
            this.Actor = actor;
        }
    }
}
