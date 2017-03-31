using System;
using System.Collections.Generic;

namespace Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }
        IEnumerable<object> UncommittedEvents { get; }
        IEnumerable<Type> EventTypes { get; }

        void Apply(object e);
        void ClearEvents();
    }
}