using System;

namespace Aggregates.Sales
{
    public class OrderItem
    {
        public OrderItem(Guid productId, decimal price)
        {
            ProductId = productId;
            Price = price;
        }

        public Guid ProductId { get; }
        public decimal Price { get; }
    }
}