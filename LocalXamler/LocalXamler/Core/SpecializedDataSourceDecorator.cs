using System;
using System.Collections.Generic; // For IEnumerable
using LocalXamler.Core; // Required for DataSource, ITimestampedData, etc.

namespace LocalXamler.Core
{
    public class SpecializedDataSourceDecorator<TItem, TValue> where TItem : ITimestampedData<TValue>
    {
        private readonly DataSource<TItem, TValue> _dataSource;

        public const string AddWithSpecialLogTriggerName = "DECORATOR_AddWithSpecialLog";
        public const string AddTransformedTriggerName = "DECORATOR_AddTransformed";

        public SpecializedDataSourceDecorator(DataSource<TItem, TValue> dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException(nameof(dataSource), "Wrapped DataSource cannot be null.");
            }
            _dataSource = dataSource;

            // Register decorator-specific triggers.
            // These triggers rely on their unique names to provide context to event subscribers.
            // The condition 'item => true' ensures they are candidates to fire whenever data is added
            // through the underlying DataSource, allowing the TriggerName to signal the context.
            _dataSource.RegisterTrigger(AddWithSpecialLogTriggerName, item => true);
            _dataSource.RegisterTrigger(AddTransformedTriggerName, item => true);
        }

        // Delegated members and specialized Add methods will be added in subsequent steps.

        #region Event Forwarding
        public event EventHandler<TriggerEventArgs<TItem>> TriggerActivated
        {
            add => _dataSource.TriggerActivated += value;
            remove => _dataSource.TriggerActivated -= value;
        }
        #endregion

        #region Delegated Read-Only Properties
        public string Name => _dataSource.Name;
        public string Id => _dataSource.Id;
        public bool IsHealthy => _dataSource.IsHealthy;
        public int Count => _dataSource.Count;
        #endregion

        #region Delegated Methods
        public bool TryDequeue(out TItem item) => _dataSource.TryDequeue(out item);
        public bool TryPeek(out TItem item) => _dataSource.TryPeek(out item);
        public void Clear() => _dataSource.Clear();
        public void SetHealthStatus(bool isHealthy) => _dataSource.SetHealthStatus(isHealthy);
        #endregion

        #region Delegated Trigger Management Methods
        public void RegisterTrigger(string triggerName, DataSource<TItem, TValue>.TriggerCondition condition) => _dataSource.RegisterTrigger(triggerName, condition);
        public bool UnregisterTrigger(string triggerName) => _dataSource.UnregisterTrigger(triggerName);
        public void ClearAllTriggers() => _dataSource.ClearAllTriggers();
        public IEnumerable<string> GetRegisteredTriggerNames() => _dataSource.GetRegisteredTriggerNames();
        #endregion

        #region Specialized Add Methods
        public void AddWithSpecialLog(TItem item, string customLogMessage)
        {
            // Assuming TItem might have a null value if it's a class, 
            // but the item structure itself (TItem) is provided.
            // A null check for 'item' itself could be added if TItem is a reference type
            // and null is not a valid item to add.
            // e.g., if (item == null) throw new ArgumentNullException(nameof(item));

            if (customLogMessage == null)
            {
                customLogMessage = string.Empty; 
            }

            // Ensure item.Value is accessed safely if Value can be null.
            // This depends on TValue. For simplicity, direct access is used.
            Console.WriteLine($"[DataSourceDecorator] Special Log: '{customLogMessage}' for item with value '{item.Value}' (Timestamp: {item.Timestamp})");
            
            _dataSource.Add(item); 
        }

        public void AddTransformed(TValue originalData, Func<TValue, TItem> transformationFunction, string transformationDescription)
        {
            if (transformationFunction == null)
            {
                throw new ArgumentNullException(nameof(transformationFunction), "Transformation function cannot be null.");
            }
            if (transformationDescription == null)
            {
                transformationDescription = string.Empty;
            }

            TItem item = transformationFunction(originalData);

            // Consider if 'item' itself can be null after transformation and if that's valid.
            if (item == null)
            {
                // Depending on requirements, either throw or handle this case.
                // For now, let's assume a valid item is always returned or _dataSource.Add handles nulls if permissible.
                // If null items are strictly forbidden:
                // throw new InvalidOperationException("Transformation function returned a null TItem, which cannot be added.");
            }
            
            // Ensure item.Value is accessed safely if Value can be null.
            Console.WriteLine($"[DataSourceDecorator] Data transformed via '{transformationDescription}'. Original: '{originalData}', New Item Value: '{item.Value}' (Timestamp: {item.Timestamp})");
            
            _dataSource.Add(item);
        }
        #endregion
    }
}
