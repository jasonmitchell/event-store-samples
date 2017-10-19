using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Aggregates
{
    public class AggregateRepository
    {
        private static readonly Dictionary<Type, List<Type>> AggregateEventMap = new Dictionary<Type, List<Type>>();

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
            var eventTypeMap = GetEventTypeMap(aggregate);
            var events = await streamReader.Read(StreamName<TAggregate>(aggregateId), version, eventTypeMap);

            events.ForEach(aggregate.Apply);

            return aggregate;
        }

        public async Task Save<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
        {
            var originalStreamVersion = aggregate.Version - aggregate.UncommittedEvents.Count();
            var expectedStreamVersion = originalStreamVersion == 0 ? ExpectedVersion.NoStream : originalStreamVersion - 1;

            await streamWriter.Write(StreamName<TAggregate>(aggregate.Id), aggregate.UncommittedEvents, expectedStreamVersion);
            aggregate.ClearEvents();
        }

        private static IDictionary<string, Type> GetEventTypeMap<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
        {
            var aggregateType = typeof(TAggregate);
            if (!AggregateEventMap.ContainsKey(aggregateType))
            {
                AggregateEventMap.Add(aggregateType, aggregate.EventTypes.ToList());
            }

            return AggregateEventMap[aggregateType].ToDictionary(x => x.Name, x => x);
        }

        private static string StreamName<TAggregate>(Guid aggregateId) where TAggregate : class, IAggregate
        {
            return $"{typeof(TAggregate).Name}-{aggregateId}";
        }
    }
}