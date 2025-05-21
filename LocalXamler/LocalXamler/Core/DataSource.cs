using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LocalXamler.Core
{
    public class DataSource<T>
    {
        public string Name { get; private set; }
        public string Id { get; private set; }
        public bool IsHealthy { get; private set; }

        private readonly ConcurrentQueue<T> _dataQueue;

        public DataSource(string name, string id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException("DataSource name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException("DataSource ID cannot be null or whitespace.", nameof(id));
            }

            Name = name;
            Id = id;
            IsHealthy = true; // Default health status
            _dataQueue = new ConcurrentQueue<T>();
        }

        public void SetHealthStatus(bool isHealthy)
        {
            IsHealthy = isHealthy;
        }

        public void Add(T item)
        {
            _dataQueue.Enqueue(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new System.ArgumentNullException(nameof(items), "Items collection cannot be null.");
            }
            foreach (var item in items)
            {
                _dataQueue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T item)
        {
            return _dataQueue.TryDequeue(out item);
        }

        public bool TryPeek(out T item)
        {
            return _dataQueue.TryPeek(out item);
        }

        public void Clear()
        {
            // Assuming ConcurrentQueue<T>.Clear() is available.
            // If not, this would need to be: while (_dataQueue.TryDequeue(out _)) { }
            _dataQueue.Clear();
        }

        public int Count => _dataQueue.Count;
    }
}
