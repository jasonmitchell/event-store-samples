using System;

namespace Streams
{
    public class SomeEvent
    {
        public SomeEvent(Guid a, int b, string c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Guid A { get; }
        public int B { get; }
        public string C { get; }
    }
}