using Darkrit.Base;
using Darkrit.Math;

namespace Darkrit.Physics.Boxy2D;

/// <summary>
/// Item of <see cref="World{T}"/>
/// </summary>
/// <typeparam name="T">The type of the custom <see cref="UserData"/>. If none is wanted use an empty struct</typeparam>
public struct Body<T> : IHandle<Body<T>>
{
    public Handle<Body<T>> Handle { get; set; }

    /// <summary>
    /// Actual AABB collider, readonly. To modify use <see cref="World{T}"/> methods
    /// </summary>
    public RectangleF Bounds { get; internal set; } // This must not be directly modified EVER
    
    /// <summary>
    /// Layer this Body is in
    /// </summary>
    public uint Layer;

    /// <summary>
    /// Layer this Body scans for
    /// </summary>
    public uint Mask;

    /// <summary>
    /// Optional userData for more personalized behaviour
    /// </summary>
    public T UserData;
}
