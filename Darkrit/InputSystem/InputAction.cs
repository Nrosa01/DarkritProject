using System;
using System.Collections.Generic;
using System.Linq;
using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

/// <summary>
/// Represents an input action with one or more bindings
/// It currently works making an OR of all bindings, Compound modes will be added soon
/// </summary>
public class InputAction(string name, IInputProvider provider)
{
    private readonly List<IInputBinding> _bindings = [];

    public string Name => name;
    public IReadOnlyList<IInputBinding> Bindings => _bindings;

    public InputAction AddBinding(IInputBinding binding)
    {
        _bindings.Add(binding);
        
        binding.provider = provider;
        
        return this;
    }
    public InputAction AddBindings(IEnumerable<IInputBinding> bindings)
    {
        _bindings.AddRange(bindings);
        
        foreach (var binding in bindings)
            binding.provider = provider;
        
        return this;
    }
    public void ClearBindings() => _bindings.Clear();

    /// <summary>
    /// True if at least one binding is pressed
    /// </summary>
    public bool IsPressed => _bindings.Any(static b => b.Pressed());

    /// <summary>
    /// True if at least one binding was pressed this frame
    /// </summary>
    public bool WasPressedThisFrame => _bindings.Any(static b => b.PressedThisFrame());

    /// <summary>
    /// True if at least one binding was freed this frame
    /// </summary>
    public bool WasReleasedThisFrame => _bindings.Any(static b => b.ReleasedThisFrame());


    /// <summary>
    /// Returns the first value different from 0 of all the bindings
    /// </summary>
    /// <returns>Value in the range [0,1]</returns>
    public float GetValue()
    {
        foreach (var binding in _bindings)
        {
            float value = binding.GetValue();
            if (MathF.Abs(value) > 0.001f)
                return value;
        }
        return 0f;
    }

    /// <summary>
    /// Returns the max absolute value of all the bindings
    /// </summary>
    public float GetMaxValue()
    {
        float max = 0f;
        foreach (var binding in _bindings)
        {
            float value = binding.GetValue();
            if (MathF.Abs(value) > MathF.Abs(max))
                max = value;
        }
        return max;
    }
}