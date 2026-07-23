using Darkrit.Input.Providers;
using System.Collections.Generic;


namespace Darkrit.Input;

/// <summary>
/// Storage for InputActions
/// Mainly a thin wrapper over a dictionary
/// </summary>
public class InputMap
{
    private readonly Dictionary<string, InputAction> _actions = [];

    /// <summary>
    /// Adds a new action to the map
    /// </summary>
    public InputAction AddAction(string name, IInputProvider provider)
    {
        var action = new InputAction(name, provider);

        if (!_actions.ContainsKey(name))
            _actions[name] = action;
        
        return action;
    }

    /// <summary>
    /// Gets an action by its name
    /// </summary>
    public InputAction GetAction(string name)
    {
        _actions.TryGetValue(name, out var action);
        return action;
    }


    /// <summary>
    /// Removes an action
    /// </summary>
    public bool RemoveAction(string name) => _actions.Remove(name);

    /// <summary>
    /// Clears all actions
    /// </summary>
    public void Clear() => _actions.Clear();
}