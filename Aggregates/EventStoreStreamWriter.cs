using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Aggregates
{
    internal class EventStoreStreamWriter
    {
        private const int WritePageSize = 500;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        private readonly IEventStoreConnection connection;

        public EventStoreStreamWriter(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public Task Write(string streamName, IEnumerable<object> events, int expectedStreamVersion)
        {
            var newEvents = events.ToList();
            var correlationId = Guid.NewGuid();
            var eventsToSave = newEvents.Select(e => ToEventData(e, correlationId)).ToList();

            if (eventsToSave.Count >= WritePageSize)
            {
                return WriteEventsInPages(streamName, expectedStreamVersion, eventsToSave);
            }

            return connection.AppendToStreamAsync(streamName, expectedStreamVersion, eventsToSave);
        }

        private async Task WriteEventsInPages(string streamName, int expectedStreamVersion, List<EventData> eventsToSave)
        {
            var transaction = await connection.StartTransactionAsync(streamName, expectedStreamVersion);

            var position = 0;
            while (position < eventsToSave.Count)
            {
                var pageEvents = eventsToSave.Skip(position).Take(WritePageSize);
                await transaction.WriteAsync(pageEvents);

                position += WritePageSize;
            }

            await transaction.CommitAsync();
        }

        private static EventData ToEventData(object e, Guid correlationId)
        {
            var metadata = new Dictionary<string, object>
            {
                ["$correlationId"] = correlationId,
                ["Type"] = e.GetType().FullName
            };

            return new EventData(Guid.NewGuid(), e.GetType().Name, true, Serialize(e), Serialize(metadata));
        }

        private static byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, SerializerSettings));
        }
    }
}