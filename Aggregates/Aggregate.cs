using System;
using System.Collections.Generic;

namespace Aggregates
{
    public abstract class Aggregate : IAggregate
    {
        private readonly Dictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>();
        private readonly Queue<object> uncommittedEvents = new Queue<object>();
        private int version;

        public Guid Id { get; protected set; }
        int IAggregate.Version => version;
        IEnumerable<object> IAggregate.UncommittedEvents => uncommittedEvents;

        protected void Given<TEvent>(Action<TEvent> handler)
        {
            handlers.Add(typeof(TEvent), x => handler((TEvent)x));
        }

        protected void Then<TEvent>(TEvent e)
        {
            ((IAggregate)this).Apply(e);
            uncommittedEvents.Enqueue(e);
        }

        void IAggregate.Apply(object e)
        {
            handlers[e.GetType()](e);
            version++;
        }

        void IAggregate.ClearUncommittedEvents()
        {
            uncommittedEvents.Clear();
        }
    }
}