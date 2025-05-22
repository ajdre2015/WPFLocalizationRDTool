using System;

namespace LocalXamler.Core
{
    public class StateDefinition
    {
        public string Name { get; }
        public Action<string, string> OnEnterAction { get; set; } // Parameters: (string fromStateName, string triggerInput)
        public Action<string, string> OnExitAction { get; set; }  // Parameters: (string toStateName, string triggerInput)

        public StateDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("State name cannot be null or whitespace.", nameof(name));
            }
            Name = name;
        }
    }

    public class TransitionDefinition
    {
        public string FromStateName { get; }
        public string Input { get; } // Input that triggers the transition
        public string ToStateName { get; }
        public Action<string, string> TransitionAction { get; set; } // Parameters: (string fromStateName, string toStateName)

        public TransitionDefinition(string fromStateName, string input, string toStateName)
        {
            if (string.IsNullOrWhiteSpace(fromStateName))
            {
                throw new ArgumentException("FromState name cannot be null or whitespace.", nameof(fromStateName));
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
            }
            if (string.IsNullOrWhiteSpace(toStateName))
            {
                throw new ArgumentException("ToState name cannot be null or whitespace.", nameof(toStateName));
            }

            FromStateName = fromStateName;
            Input = input;
            ToStateName = toStateName;
        }
    }
}
