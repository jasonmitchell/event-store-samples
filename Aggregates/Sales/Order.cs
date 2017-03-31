﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aggregates.Sales
{
    public class Order : Aggregate
    {
        public ReadOnlyCollection<OrderItem> Items { get; private set; }
        public bool Paid { get; private set; }
        public bool Delivered { get; private set; }
        public decimal TotalValue => Items.Sum(x => x.Price);

        private Order()
        {
            Given<OrderPlaced>(e =>
            {
                Id = e.Id;
                Items = new ReadOnlyCollection<OrderItem>(e.Items);
            });

            Given<PaymentReceived>(e => Paid = true);
            Given<OrderDelivered>(e => Delivered = true);
        }

        public Order(Guid id, List<OrderItem> items) : this()
        {
            Then(new OrderPlaced(id, items));
        }

        public void Pay()
        {
            Then(new PaymentReceived(Id, TotalValue));
        }

        public void DeliveredToRecipient()
        {
            Then(new OrderDelivered(Id));
        }
    }
}