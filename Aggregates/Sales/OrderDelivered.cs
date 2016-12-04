using System;

namespace Aggregates.Sales
{
    public class OrderDelivered
    {
        public OrderDelivered(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }
}