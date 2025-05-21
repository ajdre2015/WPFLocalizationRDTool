using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LocalXamler.Core
{
    public class DataSource<TItem, TValue> where TItem : ITimestampedData<TValue>
    {
        public delegate bool TriggerCondition(TItem item);

        public event EventHandler<TriggerEventArgs<TItem>> TriggerActivated;

        private readonly ConcurrentDictionary<string, TriggerCondition> _triggers = new ConcurrentDictionary<string, TriggerCondition>();

        public string Name { get; private set; }
        public string Id { get; private set; }
        public bool IsHealthy { get; private set; }

        private readonly ConcurrentQueue<TItem> _dataQueue;

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
            _dataQueue = new ConcurrentQueue<TItem>();
        }

        public void SetHealthStatus(bool isHealthy)
        {
            IsHealthy = isHealthy;
        }

        public void RegisterTrigger(string triggerName, TriggerCondition condition)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                throw new System.ArgumentException("Trigger name cannot be null or whitespace.", nameof(triggerName));
            }
            if (condition == null)
            {
                throw new System.ArgumentNullException(nameof(condition), "Trigger condition cannot be null.");
            }
            _triggers.AddOrUpdate(triggerName, condition, (key, oldCondition) => condition);
        }

        public bool UnregisterTrigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                return false; 
            }
            return _triggers.TryRemove(triggerName, out _);
        }

        public void ClearAllTriggers()
        {
            _triggers.Clear();
        }

        public IEnumerable<string> GetRegisteredTriggerNames()
        {
            return _triggers.Keys;
        }

        public void Add(TItem item)
        {
            _dataQueue.Enqueue(item); // Enqueue the item first

            // Iterate through all registered triggers.
            // ConcurrentDictionary can be safely iterated even if modified concurrently from other threads in many scenarios.
            // The items returned by GetEnumerator reflect the state of the dictionary at some point in time.
            foreach (var triggerEntry in _triggers) 
            {
                // triggerEntry is KeyValuePair<string, TriggerCondition>
                // triggerEntry.Key is the trigger name (string)
                // triggerEntry.Value is the TriggerCondition delegate

                if (triggerEntry.Value(item)) // Check the condition using the delegate
                {
                    // Fire the new event, TriggerActivated, with TriggerEventArgs
                    TriggerActivated?.Invoke(this, new TriggerEventArgs<TItem>(triggerEntry.Key, item));
                }
            }
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            if (items == null)
            {
                throw new System.ArgumentNullException(nameof(items), "Items collection cannot be null.");
            }
            foreach (var item in items) // Outer loop for each item in the input collection
            {
                _dataQueue.Enqueue(item); // Enqueue the current item

                // Inner loop: Iterate through all registered triggers for the current item.
                // ConcurrentDictionary can be safely iterated.
                foreach (var triggerEntry in _triggers)
                {
                    // triggerEntry is KeyValuePair<string, TriggerCondition>
                    // triggerEntry.Key is the trigger name (string)
                    // triggerEntry.Value is the TriggerCondition delegate

                    if (triggerEntry.Value(item)) // Check the condition using the delegate
                    {
                        // Fire the new event, TriggerActivated, with TriggerEventArgs
                        TriggerActivated?.Invoke(this, new TriggerEventArgs<TItem>(triggerEntry.Key, item));
                    }
                }
            }
        }

        public bool TryDequeue(out TItem item)
        {
            return _dataQueue.TryDequeue(out item);
        }

        public bool TryPeek(out TItem item)
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
