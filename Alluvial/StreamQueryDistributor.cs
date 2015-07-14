using System;
using System.Threading.Tasks;
using Alluvial.Distributors;

namespace Alluvial
{
    public static class StreamQueryDistributor
    {
        public static IStreamQueryDistributor Trace(this IStreamQueryDistributor distributor)
        {
            return Create(
                start: () =>
                {
                    System.Diagnostics.Trace.WriteLine("[Distribute] Start");
                    return distributor.Start();
                },
                onReceive: onReceive =>
                {
                    // FIX: (Trace) this doesn't do anything if OnReceive was called before Trace, so a proper pipeline model may be better here.
                    distributor.OnReceive(async lease =>
                    {
                        System.Diagnostics.Trace.WriteLine("[Distribute] OnReceive " + lease);
                        await onReceive(lease);
                        System.Diagnostics.Trace.WriteLine("[Distribute] OnReceive (done) " + lease);
                    });
                }, stop: () =>
                {
                    System.Diagnostics.Trace.WriteLine("[Distribute] Stop");
                    return distributor.Stop();
                }, distribute: distributor.Distribute);
        }

        private static IStreamQueryDistributor Create(Func<Task> start, Action<Func<Lease, Task>> onReceive, Func<Task> stop, Func<int, Task> distribute)
        {
            return new AnonymousStreamQueryDistributor(start, onReceive, stop, distribute);
        }
    }
}