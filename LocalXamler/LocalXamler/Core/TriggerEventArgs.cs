using System;

namespace LocalXamler.Core
{
    public class TriggerEventArgs<TItem> : EventArgs
    {
        public string TriggerName { get; }
        public TItem Item { get; }

        public TriggerEventArgs(string triggerName, TItem item)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                throw new ArgumentException("Trigger name cannot be null or whitespace.", nameof(triggerName));
            }
            // Item validity depends on TItem's nature (value/reference type)
            // and specific requirements, so no generic null check here.
            
            TriggerName = triggerName;
            Item = item;
        }
    }
}
