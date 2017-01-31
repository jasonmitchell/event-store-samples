﻿using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Aggregates
{
    public class AggregateRepository
    {
        private readonly EventStoreStreamReader streamReader;
        private readonly EventStoreStreamWriter streamWriter;

        public AggregateRepository(IEventStoreConnection connection)
        {
            streamReader = new EventStoreStreamReader(connection);
            streamWriter = new EventStoreStreamWriter(connection);
        }

        public async Task<TAggregate> Load<TAggregate>(Guid aggregateId, int version) where TAggregate : class, IAggregate
        {
            var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
            var events = await streamReader.Read(StreamName<TAggregate>(aggregateId), version);

            events.ForEach(aggregate.Apply);

            return aggregate;
        }

        public Task Save<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
        {
            var originalStreamVersion = aggregate.Version - aggregate.UncommittedEvents.Count();
            var expectedStreamVersion = originalStreamVersion == 0 ? ExpectedVersion.NoStream : originalStreamVersion - 1;

            return streamWriter.Write(StreamName<TAggregate>(aggregate.Id), aggregate.UncommittedEvents, expectedStreamVersion);
        }

        private static string StreamName<TAggregate>(Guid aggregateId) where TAggregate : class, IAggregate
        {
            return $"{typeof(TAggregate).Name}-{aggregateId}";
        }
    }
}