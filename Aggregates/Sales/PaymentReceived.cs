using System;

namespace Aggregates.Sales
{
    public class PaymentReceived
    {
        public PaymentReceived(Guid orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
        }

        public Guid OrderId { get; }
        public decimal Amount { get; }
    }
}