using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Aggregates
{
    internal class EventStoreStreamReader
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        private readonly IEventStoreConnection connection;

        public EventStoreStreamReader(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public async Task<List<object>> Read(string streamName, int version)
        {
            var events = new List<object>();
            var sliceStart = 0;
            StreamEventsSlice slice;

            do
            {
                slice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, SliceSize(version, sliceStart), false);
                sliceStart = slice.NextEventNumber;

                events.AddRange(slice.Events.Select(Deserialize));
            } while (version >= slice.NextEventNumber && !slice.IsEndOfStream);

            return events;
        }

        private static int SliceSize(int version, int sliceStart)
        {
            const int readPageSize = 500;
            return sliceStart + readPageSize <= version ? readPageSize : version - sliceStart + 1;
        }

        private static object Deserialize(ResolvedEvent resolvedEvent)
        {
            var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));
            var eventType = Assembly.GetExecutingAssembly().GetType(metadata["Type"].ToString(), true);

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), eventType, SerializerSettings);
        }
    }
}