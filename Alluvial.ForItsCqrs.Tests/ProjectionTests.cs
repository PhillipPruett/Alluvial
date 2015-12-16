﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Alluvial.Distributors;
using Alluvial.Distributors.Sql;
using Microsoft.Its.Domain;
using Microsoft.Its.Domain.Sql;
using Microsoft.Its.Domain.Testing;
using NUnit.Framework;
using Pocket;

namespace Alluvial.ForItsCqrs.Tests
{
    [TestFixture]
    public class ProjectionTests
    {
        private static int absoluteSequenceNumber;
        private readonly InMemoryEventStream eventStream = new InMemoryEventStream();

        [Test]
        public async Task AllChanges_doesnt_miss_aggregates()
        {
            var aggregateId1 = Guid.NewGuid();
            var aggregateId2 = Guid.NewGuid();
            var aggregateId3 = Guid.NewGuid();
            var aggregateId4 = Guid.NewGuid();

            var storableEvents = CreateStorableEvents(aggregateId1, aggregateId2, aggregateId3, aggregateId4).AsQueryable();
            var allChanges = EventStream.PerAggregate("Snarf", () => storableEvents);

            var aggregator = Aggregator.Create<int, IEvent>((oldCount, batch) =>
            {
                var eventType14s = batch.OfType<AggregateB.EventType14>();
                var newCount = eventType14s.Count();
                return oldCount + newCount;
            }).Trace();

            var catchup = StreamCatchup.All(allChanges);

            var count = 0;
            var store = ProjectionStore.Create<string, int>(async _ => count, async (_, newCount) => count = newCount);
            catchup.Subscribe(aggregator, store);
            catchup.RunUntilCaughtUp().Wait();
            count.Should().Be(2);
        }

        [Test]
        public async Task Map_projections_have_individual_cursors()
        {
            var aggregateIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToArray();

            await WriteEvents<AggregateA.EventType1>(aggregateIds.Concat(aggregateIds));

            var allChanges = EventStream.PerAggregate("All",
                                                      () => eventStream
                                                          .Select(e => e.ToStorableEvent())
                                                          .AsQueryable());

            var catchup = StreamCatchup.All(allChanges);

            var aggregator = Aggregator.Create<Projection<int, long>, IEvent>((p, b) =>
            {
                p.Value++;
                Console.WriteLine(b);
            });

            var store = new InMemoryProjectionStore<Projection<int, long>>();
            catchup.Subscribe(aggregator, store);

            await catchup.RunUntilCaughtUp().TimeoutAfter(DefaultTimeout());

            store.Select(p => p.CursorPosition).Should().OnlyContain(i => i > 100);
        }

        [Test]
        public async Task When_one_map_projection_encounters_errors_it_does_not_cause_the_others_to_fall_behind()
        {



            // FIX (When_one_map_projection_encounters_errors_the_others_do_not_fall_behind) write test
            Assert.Fail("Test not written yet.");
        }

        [Test]
        public async Task The_partition_that_is_the_farthest_behind_is_picked_up_first_by_the_distributor()
        {
            var database = new SqlBrokeredDistributorDatabase(
                @"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=AlluvialSqlDistributorTests");

            var leaseables = Partition.AllGuids()
                                      .Among(10)
                                      .Select(p => new Leasable<IStreamQueryRangePartition<Guid>>(p, p.ToString())
                                      {
                                          LeaseLastGranted = DateTimeOffset.Parse("2015-12-16 06:29:53 AM"),
                                          LeaseLastReleased = DateTimeOffset.Parse("2015-12-16 06:31:11 AM")
                                      })
                                      .ToArray();

            await database.CreateDatabase();
            var pool = Guid.NewGuid().ToString();
            await database.RegisterLeasableResources(leaseables, pool);

            var distributor = new SqlBrokeredDistributor<IStreamQueryRangePartition<Guid>>(
                leaseables,
                database,
                pool,
                5,
                TimeSpan.FromSeconds(30));

            DateTimeOffset lastGranted = new DateTimeOffset(); 
            
            distributor.OnReceive(async lease =>
            {
                lastGranted = lease.LastGranted;
            });

            await distributor.Distribute(1);

            Console.WriteLine(lastGranted);


            // FIX (The_projections_that_are_the_farthest_behind_are_updated_first) write test
            Assert.Fail("Test not written yet.");
        }

        private static StorableEvent[] CreateStorableEvents(
            Guid aggregateId1,
            Guid aggregateId2,
            Guid aggregateId3,
            Guid aggregateId4)
        {
            return new[]
            {
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType1).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 1,
                    Id = 7,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType2).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 2,
                    Id = 8,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType3).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 3,
                    Id = 9,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType4).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 4,
                    Id = 10,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType5).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 5,
                    Id = 11,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 6,
                    Id = 12,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 7,
                    Id = 13,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 8,
                    Id = 14,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 9,
                    Id = 15,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 10,
                    Id = 16,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 11,
                    Id = 18,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 12,
                    Id = 19,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 13,
                    Id = 20,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 14,
                    Id = 21,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 15,
                    Id = 22,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId1,
                    SequenceNumber = 16,
                    Id = 23,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType1).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 1,
                    Id = 24,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType9).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 2,
                    Id = 25,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType10).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 3,
                    Id = 26,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType11).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 4,
                    Id = 27,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType12).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 5,
                    Id = 28,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType13).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 6,
                    Id = 29,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType14).Name,
                    AggregateId = aggregateId2,
                    SequenceNumber = 7,
                    Id = 40,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType1).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 1,
                    Id = 1,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType9).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 2,
                    Id = 2,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType10).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 3,
                    Id = 3,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType11).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 4,
                    Id = 4,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType12).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 5,
                    Id = 5,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType13).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 6,
                    Id = 6,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateB).Name,
                    Type = typeof (AggregateB.EventType14).Name,
                    AggregateId = aggregateId3,
                    SequenceNumber = 7,
                    Id = 17,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType1).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 1,
                    Id = 30,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType2).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 2,
                    Id = 31,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType3).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 3,
                    Id = 32,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType4).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 4,
                    Id = 33,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType5).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 5,
                    Id = 34,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 6,
                    Id = 35,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 7,
                    Id = 36,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 8,
                    Id = 37,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 9,
                    Id = 38,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 10,
                    Id = 39,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 11,
                    Id = 41,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 12,
                    Id = 42,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 13,
                    Id = 43,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType8).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 14,
                    Id = 44,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType6).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 15,
                    Id = 45,
                    Body = "{}"
                },
                new StorableEvent
                {
                    StreamName = typeof (AggregateA).Name,
                    Type = typeof (AggregateA.EventType7).Name,
                    AggregateId = aggregateId4,
                    SequenceNumber = 16,
                    Id = 46,
                    Body = "{}"
                },
            };
        }

        private TimeSpan DefaultTimeout()
        {
            return TimeSpan.FromSeconds(5*(Debugger.IsAttached ? 100 : 1));
        }

        private async Task WriteEvents<TEvent>(IEnumerable<Guid> aggregateIds) where TEvent : IEvent, new()
        {
            foreach (var aggregateId in aggregateIds)
            {
                var existing = new EventSequence(aggregateId);

                existing.AddRange((await eventStream.All(aggregateId.ToString()))
                                      .Select(e => e.ToDomainEvent()));

                var @event = new TEvent();

                existing.Add(@event);

                var storedEvent = @event.ToStoredEvent() as InMemoryStoredEvent;

                storedEvent.Metadata.AbsoluteSequenceNumber = eventStream.Count() + 1;

                await eventStream.Append(new[] { storedEvent });
            }
        }
    }

    public static class EventExtensions
    {
        public static StorableEvent ToStorableEvent(this IStoredEvent @event)
        {
            var absoluteSequenceNumber = @event.IfTypeIs<IHaveExtensibleMetada>()
                                               .And()
                                               .IfHas<int>(e => e.Metadata.AbsoluteSequenceNumber)
                                               .ElseDefault();

            return new StorableEvent
            {
                SequenceNumber = @event.SequenceNumber,
                AggregateId = Guid.Parse(@event.AggregateId),
                Timestamp = @event.Timestamp,
                Type = @event.Type,
                Body = @event.Body,
                ETag = @event.ETag,
                StreamName = @event.StreamName,
                Id = absoluteSequenceNumber
            };
        }
    }
}