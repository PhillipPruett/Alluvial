using System;

namespace Alluvial.Distributors
{
    public class LeasableResource
    {
        public LeasableResource(string name, TimeSpan defaultDuration)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (defaultDuration == new TimeSpan())
            {
                throw new ArgumentException("duration cannot be zero.");
            }
            Name = name;
            DefaultDuration = defaultDuration;
        }

        public TimeSpan DefaultDuration { get; set; }

        public string Name { get; private set; }

        public DateTimeOffset LeaseLastGranted { get; set; }

        public DateTimeOffset LeaseLastReleased { get; set; }

        public override string ToString()
        {
            return string.Format("leasable resource:{0} (granted @ {1}, released @ {2})",
                                 Name,
                                 LeaseLastGranted,
                                 LeaseLastReleased);
        }
    }
}