using System;
using System.Collections.Generic;
using Aggregates.Sales;
using FluentAssertions;
using Xunit;

namespace Aggregates
{
    public class AggregateTests
    {
        [Fact]
        public void TracksUncommittedEvents()
        {
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), 10),
                new OrderItem(Guid.NewGuid(), 15),
                new OrderItem(Guid.NewGuid(), 20),
            };

            var order = new Order(orderId, orderItems);
            order.Pay();
            order.DeliveredToRecipient();

            order.As<IAggregate>().UncommittedEvents.Should().HaveCount(3);
        }

        [Fact]
        public void ClearsUncommittedEvents()
        {
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), 10),
                new OrderItem(Guid.NewGuid(), 15),
                new OrderItem(Guid.NewGuid(), 20),
            };

            var order = new Order(orderId, orderItems);
            order.Pay();
            order.DeliveredToRecipient();

            var aggregate = (IAggregate) order;
            aggregate.ClearUncommittedEvents();

            aggregate.UncommittedEvents.Should().HaveCount(0);
        }

        [Fact]
        public void AppliesPastEvents()
        {
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), 10),
                new OrderItem(Guid.NewGuid(), 15),
                new OrderItem(Guid.NewGuid(), 20),
            };

            var events = new object[]
            {
                new OrderPlaced(orderId, orderItems),
                new PaymentReceived(orderId, 45),
                new OrderDelivered(orderId)
            };

            var aggregate = (IAggregate)Activator.CreateInstance(typeof(Order), true);
            aggregate.Apply(events);

            var order = (Order) aggregate;

            aggregate.Version.Should().Be(3);
            order.Id.Should().Be(orderId);
            order.Paid.Should().BeTrue();
            order.Delivered.Should().BeTrue();
        }
    }
}