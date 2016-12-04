using System;
using System.Collections.Generic;

namespace Aggregates.Sales
{
    public class OrderPlaced
    {
        public OrderPlaced(Guid id, List<OrderItem> items)
        {
            Id = id;
            Items = items;
        }

        public Guid Id { get; }
        public List<OrderItem> Items { get; }
    }
}