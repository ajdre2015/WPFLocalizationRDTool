using System;

namespace LocalXamler.Core
{
    public interface ITimestampedData<TValue>
    {
        TValue Value { get; }
        DateTimeOffset Timestamp { get; }
    }
}
