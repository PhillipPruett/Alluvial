using System;
using System.Threading.Tasks;
using Alluvial.Distributors;
using NUnit.Framework;

namespace Alluvial.Tests.Distributors
{
    [TestFixture]
    public class InMemoryStreamQueryDistributorTests : StreamQueryDistributorTests
    {
        private InMemoryDistributor distributor;

        protected override IDistributor CreateDistributor(
            Func<Lease, Task> onReceive = null,
            LeasableResource[] leasableResources = null,
            int maxDegreesOfParallelism = 5,
            string name = null,
            TimeSpan? waitInterval = null,
            string scope = null)
        {
            distributor = new InMemoryDistributor(
                leasableResources ?? DefaultLeasableResources,
                scope ?? DateTimeOffset.UtcNow.Ticks.ToString(),
                maxDegreesOfParallelism,
                waitInterval);
            if (onReceive != null)
            {
                distributor.OnReceive(onReceive);
            }
            return distributor;
        }

        protected override TimeSpan DefaultLeaseDuration
        {
            get
            {
                return TimeSpan.FromSeconds(1);
            }
        }

        protected override TimeSpan ClockDriftTolerance
        {
            get
            {
                return TimeSpan.FromMilliseconds(30);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (distributor != null)
            {
                distributor.Dispose();
            }
        }
    }
}