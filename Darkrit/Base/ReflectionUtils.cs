using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Darkrit.Base;

/// <summary>
/// Reflection utils compatible with Native AOT export
/// </summary>
internal class ReflectionUtils
{
    // Source - https://stackoverflow.com/a/2362756
    // Posted by Hath, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-07-30, License - CC BY-SA 4.0

    /// <summary>
    /// Find all derived types that aren't abstract classes of the type T
    /// </summary>
    /// <typeparam name="T">The type to get derived types from</typeparam>
    /// <returns>An inmutable list of the assemblies that derive from T</returns>
    public static IReadOnlyList<Type> FindAllDerivedTypes<T>()
    {
        var baseType = typeof(T);

        return [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(static assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(static t => t != null)!;
                }
            })
            .Where(t =>
                t is { IsAbstract: false } &&
                baseType.IsAssignableFrom(t) &&
                t != baseType)];
    }
}
