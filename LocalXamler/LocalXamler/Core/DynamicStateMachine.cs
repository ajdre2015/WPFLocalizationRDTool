using System;
using System.Collections.Generic;
// Assuming DynamicStateMachineElements.cs is in LocalXamler.Core, so StateDefinition and TransitionDefinition are accessible.
// If they were in a different sub-namespace, a 'using LocalXamler.Core.Elements;' or similar would be needed.

namespace LocalXamler.Core
{
    public class DynamicStateMachine
    {
        public string CurrentStateName { get; private set; }

        private readonly Dictionary<string, StateDefinition> _states = new Dictionary<string, StateDefinition>();

        private readonly Dictionary<string, Dictionary<string, TransitionDefinition>> _transitions =
            new Dictionary<string, Dictionary<string, TransitionDefinition>>();

        public Action<string> Logger { get; set; } = message => Console.WriteLine($"[StateMachineLog] {message}");

        public DynamicStateMachine()
        {
            // CurrentStateName is initially null.
            // SetInitialState will be used to formally set the starting state.
        }

        public bool AddState(string stateName, Action<string, string> onEnter = null, Action<string, string> onExit = null)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                Logger?.Invoke($"Error: State name cannot be null or whitespace.");
                return false;
            }
            if (_states.ContainsKey(stateName))
            {
                Logger?.Invoke($"Error: State '{stateName}' already exists.");
                return false;
            }

            var newState = new StateDefinition(stateName) // Assumes StateDefinition is accessible
            {
                OnEnterAction = onEnter,
                OnExitAction = onExit
            };
            _states[stateName] = newState;
            // Also initialize the entry for this state in the _transitions dictionary if it might not exist
            // when AddTransition is called for it as a FromState.
            if (!_transitions.ContainsKey(stateName))
            {
                _transitions[stateName] = new Dictionary<string, TransitionDefinition>();
            }
            Logger?.Invoke($"State '{stateName}' added.");
            return true;
        }

        public bool RemoveState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                Logger?.Invoke($"Error: State name cannot be null or whitespace for removal.");
                return false;
            }
            if (!_states.ContainsKey(stateName))
            {
                Logger?.Invoke($"Error: State '{stateName}' not found for removal.");
                return false;
            }
            if (CurrentStateName == stateName)
            {
                Logger?.Invoke($"Error: Cannot remove current state '{stateName}'. Change current state first or stop the machine.");
                return false;
            }

            _states.Remove(stateName);

            // Remove all transitions originating from this state
            _transitions.Remove(stateName);

            // Remove all transitions pointing to this state
            var transitionsToRemove = new List<Tuple<string, string>>(); // FromStateName, InputKey
            foreach (var fromStateKey in _transitions.Keys.ToList()) // ToList to allow modification
            {
                var inputTransitions = _transitions[fromStateKey];
                foreach (var inputKey in inputTransitions.Keys.ToList()) // ToList to allow modification
                {
                    if (inputTransitions[inputKey].ToStateName == stateName)
                    {
                        transitionsToRemove.Add(new Tuple<string, string>(fromStateKey, inputKey));
                    }
                }
            }

            foreach (var transToRemove in transitionsToRemove)
            {
                if (_transitions.TryGetValue(transToRemove.Item1, out var inputTransitionsMap))
                {
                    inputTransitionsMap.Remove(transToRemove.Item2);
                    if (inputTransitionsMap.Count == 0)
                    {
                        _transitions.Remove(transToRemove.Item1);
                    }
                }
            }
            Logger?.Invoke($"State '{stateName}' and its associated transitions removed.");
            return true;
        }

        public StateDefinition GetState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                Logger?.Invoke($"Error: State name cannot be null or whitespace for GetState.");
                return null;
            }
            if (_states.TryGetValue(stateName, out var stateDef))
            {
                return stateDef;
            }
            Logger?.Invoke($"Info: State '{stateName}' not found during GetState.");
            return null;
        }

        public void SetInitialState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                Logger?.Invoke("Error: Initial state name cannot be null or whitespace.");
                return; // Or throw ArgumentException
            }
            if (!_states.ContainsKey(stateName))
            {
                Logger?.Invoke($"Error: State '{stateName}' does not exist. Cannot set as initial state. Add the state first.");
                return; // Or throw InvalidOperationException
            }
            CurrentStateName = stateName;
            Logger?.Invoke($"Initial state set to '{stateName}'. No actions are executed on initial set.");
        }

        public bool AddTransition(string fromStateName, string input, string toStateName, Action<string, string> transitionAction = null)
        {
            if (string.IsNullOrWhiteSpace(fromStateName) || string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(toStateName))
            {
                Logger?.Invoke("Error: FromState, ToState, and Input cannot be null or whitespace for AddTransition.");
                return false;
            }
            if (!_states.ContainsKey(fromStateName))
            {
                Logger?.Invoke($"Error: FromState '{fromStateName}' does not exist. Cannot add transition. Add the state first.");
                return false;
            }
            if (!_states.ContainsKey(toStateName))
            {
                Logger?.Invoke($"Error: ToState '{toStateName}' does not exist. Cannot add transition. Add the state first.");
                return false;
            }

            // Ensure the outer dictionary for fromStateName exists.
            // This should have been handled by AddState, but as a defensive measure:
            if (!_transitions.ContainsKey(fromStateName))
            {
                _transitions[fromStateName] = new Dictionary<string, TransitionDefinition>();
            }

            if (_transitions[fromStateName].ContainsKey(input))
            {
                Logger?.Invoke($"Error: Transition from '{fromStateName}' on input '{input}' already exists. Policy: Not overwriting. Remove it first.");
                return false; 
            }

            var newTransition = new TransitionDefinition(fromStateName, input, toStateName) // Assumes TransitionDefinition is accessible
            {
                TransitionAction = transitionAction
            };
            _transitions[fromStateName][input] = newTransition;
            Logger?.Invoke($"Transition added: {fromStateName} --({input})--> {toStateName}.");
            return true;
        }

        public bool RemoveTransition(string fromStateName, string input)
        {
            if (string.IsNullOrWhiteSpace(fromStateName) || string.IsNullOrWhiteSpace(input))
            {
                Logger?.Invoke("Error: FromState and Input cannot be null or whitespace for RemoveTransition.");
                return false;
            }

            if (_transitions.TryGetValue(fromStateName, out var stateTransitionsForFromState))
            {
                if (stateTransitionsForFromState.Remove(input))
                {
                    Logger?.Invoke($"Transition removed: {fromStateName} --({input})--> [Removed]");
                    // If this state now has no outgoing transitions, remove its entry from the main dictionary.
                    if (stateTransitionsForFromState.Count == 0)
                    {
                        _transitions.Remove(fromStateName);
                    }
                    return true;
                }
            }
            Logger?.Invoke($"Error: Transition from '{fromStateName}' on input '{input}' not found for removal, or state itself has no transitions defined.");
            return false;
        }

        public bool ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(CurrentStateName))
            {
                Logger?.Invoke("Error: Current state is not set or is invalid. Call SetInitialState with a valid state name first.");
                return false;
            }
            if (!_states.ContainsKey(CurrentStateName)) // Should not happen if SetInitialState was used correctly
            {
                Logger?.Invoke($"Error: Current state '{CurrentStateName}' is set but no longer exists in the state definitions. This indicates an inconsistency.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                Logger?.Invoke("Error: Input to process cannot be null or whitespace.");
                return false;
            }

            // Try to get the dictionary of transitions for the current state.
            if (!_transitions.TryGetValue(CurrentStateName, out var transitionsFromCurrentState) || transitionsFromCurrentState == null)
            {
                Logger?.Invoke($"Info: No transitions defined from state '{CurrentStateName}'. Cannot process input '{input}'.");
                return false;
            }

            // Try to get the specific transition for the given input from the current state's transitions.
            if (!transitionsFromCurrentState.TryGetValue(input, out var selectedTransition) || selectedTransition == null)
            {
                Logger?.Invoke($"Info: No transition found for input '{input}' from state '{CurrentStateName}'.");
                return false;
            }

            // A valid transition is found.
            // FromStateDefinition is guaranteed by _states.ContainsKey(CurrentStateName) check above.
            var fromStateDef = _states[CurrentStateName]; 
            
            // ToStateDefinition should be valid because AddTransition checks for ToState existence.
            // However, a defensive check here can be useful against direct manipulations or bugs.
            if (!_states.TryGetValue(selectedTransition.ToStateName, out var toStateDef) || toStateDef == null)
            {
                Logger?.Invoke($"Error: Target state '{selectedTransition.ToStateName}' for transition from '{CurrentStateName}' on input '{input}' does not exist. Transition is invalid.");
                return false;
            }

            Logger?.Invoke($"Processing input '{input}': Attempting transition from '{CurrentStateName}' to '{selectedTransition.ToStateName}'.");

            // 1. Execute OnExit action of the current (from) state.
            // Pass the name of the state being transitioned TO, and the input that triggered it.
            fromStateDef.OnExitAction?.Invoke(selectedTransition.ToStateName, input);
            Logger?.Invoke($"OnExit action for state '{CurrentStateName}' executed (if defined).");

            // 2. Execute the transition action.
            // Pass the name of the state being transitioned FROM, and the state being transitioned TO.
            selectedTransition.TransitionAction?.Invoke(CurrentStateName, selectedTransition.ToStateName);
            Logger?.Invoke($"TransitionAction for '{CurrentStateName}' --({input})--> '{selectedTransition.ToStateName}' executed (if defined).");

            string previousStateNameForOnEnter = CurrentStateName;
            CurrentStateName = selectedTransition.ToStateName; // Update current state
            Logger?.Invoke($"State changed: Current state is now '{CurrentStateName}'.");

            // 4. Execute OnEnter action of the new current (to) state.
            // Pass the name of the state being transitioned FROM, and the input that triggered it.
            toStateDef.OnEnterAction?.Invoke(previousStateNameForOnEnter, input);
            Logger?.Invoke($"OnEnter action for state '{CurrentStateName}' executed (if defined).");

            return true;
        }
    }
}
