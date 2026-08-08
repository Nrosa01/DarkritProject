// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Darkrit.Base;

/// <summary>
/// Provides hashing functions that aren't found in the standard C# libs
/// </summary>
public static class Hash
{

    /// <summary>
    /// Hashes a string as a 64 bit integer. This function performance
    /// is linearly related to the length of the string
    /// </summary>
    /// <param name="str">The string to hash</param>
    /// <returns>The hash of the string-</returns>
    public static ulong Hash64(string str)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        ulong hash = offset;

        foreach (char c in str)
        {
            hash ^= (byte)c;
            hash *= prime;

            hash ^= (byte)(c >> 8);
            hash *= prime;
        }

        return hash;
    }
}
