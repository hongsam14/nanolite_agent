using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanolite_agent.Beacon
{
    public enum ArtifectType
    {
        File,
        Registry,
        Network,
        Process,
        Module,
    }

    public enum ActorType
    {
        // process
        REMOTE_THREAD,
        TAMPERING,
        // network
        CONNECT,
        ACCEPT,
        // file system
        CREATE,
        RENAME,
        DELETE,
        MODIFY,
        CREATE_STREAM_HASH,
        // registry
        REG_ADD,
        REG_DELETE,
        REG_SET,
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
