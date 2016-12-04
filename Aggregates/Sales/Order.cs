using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aggregates.Sales
{
    public class Order : Aggregate
    {
        private Order()
        {
            Given<OrderPlaced>(Given);
            Given<PaymentReceived>(Given);
            Given<OrderDelivered>(Given);
        }

        public Order(Guid id, List<OrderItem> items) : this()
        {
            Then(new OrderPlaced(id, items));
        }

        public ReadOnlyCollection<OrderItem> Items { get; private set; }
        public bool Paid { get; private set; }
        public bool Delivered { get; private set; }
        public decimal TotalValue => Items.Sum(x => x.Price);

        private void Given(OrderPlaced e)
        {
            Id = e.Id;
            Items = new ReadOnlyCollection<OrderItem>(e.Items);
        }

        public void Pay()
        {
            Then(new PaymentReceived(Id, TotalValue));
        }

        private void Given(PaymentReceived e)
        {
            Paid = true;
        }

        public void DeliveredToRecipient()
        {
            Then(new OrderDelivered(Id));
        }

        private void Given(OrderDelivered e)
        {
            Delivered = true;
        }
    }
}