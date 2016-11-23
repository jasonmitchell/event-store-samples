using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using Newtonsoft.Json;
using Xunit;

namespace Streams
{
    public class StreamWritingAndReading : IDisposable
    {
        private readonly ClusterVNode _node;
        private readonly ConnectionSettingsBuilder _connectionSettingsBuilder;

        public StreamWritingAndReading()
        {
            _node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .RunInMemory();

            _connectionSettingsBuilder = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting();
        }

        [Fact]
        public async Task WritesEventsToStream()
        {
            await _node.StartAndWaitUntilReady();

            using (var connection = EmbeddedEventStoreConnection.Create(_node, _connectionSettingsBuilder))
            {
                var commitMetadata = new Dictionary<string, object> {{"CommitId", Guid.NewGuid()}};
                var events = new[]
                {
                    new SomeEvent(Guid.NewGuid(), 1, "One"),
                    new SomeEvent(Guid.NewGuid(), 2, "Two"),
                    new SomeEvent(Guid.NewGuid(), 3, "Three"),
                };

                var eventData = events.Select(e => ToEventData(Guid.NewGuid(), e, commitMetadata)).ToList();
                await connection.AppendToStreamAsync("test_stream", ExpectedVersion.Any, eventData);
            }
        }

        private static EventData ToEventData(Guid eventId, object e, IDictionary<string, object> metadata)
        {
            var encodedEvent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
            var encodedMetadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata));
            var typeName = e.GetType().Name;

            return new EventData(eventId, typeName, true, encodedEvent, encodedMetadata);
        }

        [Fact]
        public async Task ReadsEventsFromStream()
        {
            await _node.StartAndWaitUntilReady();

            using (var connection = EmbeddedEventStoreConnection.Create(_node, _connectionSettingsBuilder))
            {
                var commitMetadata = new Dictionary<string, object> {{"CommitId", Guid.NewGuid()}};
                var eventsToWrite = new[]
                {
                    new SomeEvent(Guid.NewGuid(), 1, "One"),
                    new SomeEvent(Guid.NewGuid(), 2, "Two"),
                    new SomeEvent(Guid.NewGuid(), 3, "Three"),
                };

                var eventData = eventsToWrite.Select(e => ToEventData(Guid.NewGuid(), e, commitMetadata)).ToList();
                await connection.AppendToStreamAsync("test_stream", ExpectedVersion.Any, eventData);

                var eventsInStream = await connection.ReadStreamEventsForwardAsync("test_stream", StreamPosition.Start, 4096, true);
                Assert.Equal(3, eventsInStream.Events.Length);
            }
        }

        public void Dispose()
        {
            _node.Stop();
        }
    }
}