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
            var eventType = typeof(TEvent);
            handlers.Add(eventType, x => handler((TEvent)x));
        }

        protected void Then<TEvent>(TEvent e)
        {
            uncommittedEvents.Enqueue(e);
            Apply(e);
        }

        void IAggregate.Apply(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                Apply(e);
                version++;
            }
        }

        private void Apply(object e)
        {
            var eventType = e.GetType();

            if (handlers.ContainsKey(eventType))
            {
                var handler = handlers[eventType];
                handler(e);
            }
        }

        void IAggregate.ClearUncommittedEvents()
        {
            uncommittedEvents.Clear();
        }
    }
}