using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Darkrit.Base
{
    internal class ReflectionUtils
    {
        // Source - https://stackoverflow.com/a/2362756
        // Posted by Hath, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-07-30, License - CC BY-SA 4.0

        public static List<Type> FindAllDerivedTypesTAssembly<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static IReadOnlyList<Type> FindAllDerivedTypes<T>()
        {
            var baseType = typeof(T);

            return AppDomain.CurrentDomain.GetAssemblies()
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
                    t != baseType)
                .ToList();
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var baseType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != baseType &&
                    baseType.IsAssignableFrom(t)
                    ).ToList();

        }

    }
}
