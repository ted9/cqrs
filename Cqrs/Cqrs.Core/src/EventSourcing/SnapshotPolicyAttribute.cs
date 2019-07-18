using System;

namespace Cqrs.EventSourcing
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class SnapshotPolicyAttribute : Attribute
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public SnapshotPolicyAttribute(int version)
        {
            if (version <= 0)
                throw new ArgumentException("Version number must be greater than zero", "version");

            this.TriggeredVersion = version;
        }

        public int TriggeredVersion { get; private set; }
    }
}
