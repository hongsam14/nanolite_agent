namespace Nanolite_agent.NanoException
{
    using System;

    [Serializable]
    public class BeaconException : Exception
    {
        public BeaconException()
        {
        }

        public BeaconException(string message)
            : base(message)
        {
        }

        public BeaconException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private BeaconException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
