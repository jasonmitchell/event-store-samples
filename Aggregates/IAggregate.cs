using System;
using System.Collections.Generic;

namespace Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }
        IEnumerable<object> UncommittedEvents { get; }

        void Apply(IEnumerable<object> events);
        void ClearUncommittedEvents();
    }
}