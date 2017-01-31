using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aggregates.Sales;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using FluentAssertions;
using Xunit;

namespace Aggregates.Tests
{
    public class OrderTests : IDisposable
    {
        private readonly ClusterVNode node;
        private readonly ConnectionSettingsBuilder connectionSettingsBuilder;

        public OrderTests()
        {
            node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .RunInMemory();

            connectionSettingsBuilder = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting();
        }

        [Fact]
        public async Task PlaceOrder()
        {
            await node.StartAndWaitUntilReady();

            using (var connection = EmbeddedEventStoreConnection.Create(node, connectionSettingsBuilder))
            {
                var repository = new AggregateRepository(connection);
                var order = new Order(Guid.NewGuid(), new List<OrderItem>
                {
                    new OrderItem(Guid.NewGuid(), 10),
                    new OrderItem(Guid.NewGuid(), 15),
                    new OrderItem(Guid.NewGuid(), 20),
                });

                await repository.Save(order);

                var eventsInStream = await connection.ReadStreamEventsForwardAsync($"Order-{order.Id}", StreamPosition.Start, 4096, true);
                eventsInStream.Events.Should().HaveCount(1)
                                     .And.ContainSingle(x => x.OriginalEvent.EventType == nameof(OrderPlaced));
            }
        }

        [Fact]
        public async Task PayForOrder()
        {
            await node.StartAndWaitUntilReady();

            using (var connection = EmbeddedEventStoreConnection.Create(node, connectionSettingsBuilder))
            {
                var repository = new AggregateRepository(connection);
                var order = new Order(Guid.NewGuid(), new List<OrderItem>
                {
                    new OrderItem(Guid.NewGuid(), 10),
                    new OrderItem(Guid.NewGuid(), 15),
                    new OrderItem(Guid.NewGuid(), 20),
                });

                await repository.Save(order);

                order = await repository.Load<Order>(order.Id, int.MaxValue);
                order.Pay();
            }
        }

        public void Dispose()
        {
            node.StopNonblocking(true, true);
        }
    }
}