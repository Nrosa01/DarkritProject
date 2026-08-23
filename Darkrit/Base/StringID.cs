// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// The following code was adapted from: https://github.com/BobbyAnguelov/Esoterica/blob/main/Code/Base/Types/StringID.h

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Darkrit.Base;

/// <summary>
/// Class that encapsulates strings with IDs in a map.
/// It is thread safe
/// </summary>
public readonly struct StringID : IEquatable<StringID>
{
    /// <summary>
    /// Invalid StringID
    /// </summary>
    public static readonly StringID Invalid = new(0);

    private static readonly ConcurrentDictionary<ulong, string> Cache = [];

    /// <summary>
    /// ID that represents the internal string of this <see cref="StringID"/>
    /// </summary>
    public readonly ulong ID;

    /// <summary>
    /// Default constructor that creates an invalid <seealso cref="StringID"/>
    /// </summary>
    public StringID() => ID = 0;

    /// <summary>
    /// Constructs a <see cref="StringID"/> from the <paramref name="str"/>
    /// For two equal strings, the generate ID is the same as is cached, but
    /// it's not guaranteed to be the same ID across execution
    /// </summary>
    /// <param name="str"></param>
    public StringID(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            ID = 0;
            return;
        }

        ID = Hash.Hash64(str);

    #if DEBUG
        if (Cache.TryGetValue(ID, out var existing))
            Debug.Assert(existing == str);
    #endif

        Cache.TryAdd(ID, str);
    }

    /// <summary>
    /// Get the string associated with this StringID
    /// </summary>
    /// <returns></returns>
    public readonly string GetString()
    {
        return Cache.TryGetValue(ID, out var str)
            ? str
            : null;
    }

    /// <summary>
    /// Gets the stored name in this <see cref="StringID"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() => GetString() ?? "";

    /// <summary>
    /// Constructs a StringID given an existing id.
    /// This method doesn't check that the id is
    /// associated with an existing string. If you then do
    /// <see cref="GetString"/> you might get null
    /// </summary>
    /// <param name="id"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringID(ulong id) => ID = id;

    /// <summary>
    /// Whether there is valid name in this <see cref="StringID"/>
    /// </summary>
    public readonly bool IsValid => ID != 0;
    
    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is StringID ID && Equals(ID);

    /// <inheritdoc/>
    public static bool operator ==(StringID a, StringID b) => a.ID == b.ID;

    /// <inheritdoc/>
    public static bool operator !=(StringID a, StringID b) => a.ID != b.ID;

    /// <inheritdoc/>
    public readonly override int GetHashCode() => ID.GetHashCode();

    /// <inheritdoc/>
    public readonly bool Equals(StringID other) => other.ID == ID;
}
