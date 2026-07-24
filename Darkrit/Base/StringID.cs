// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// The following code was adapted from: https://github.com/BobbyAnguelov/Esoterica/blob/main/Code/Base/Types/StringID.h

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Darkrit.Base
{
    public readonly struct StringID : IEquatable<StringID>
    {
        public static readonly StringID Invalid = new(0);

        private static readonly ConcurrentDictionary<ulong, string> Cache = [];

        public readonly ulong ID;

        public StringID() => ID = 0;
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

        public readonly string GetString()
        {
            return Cache.TryGetValue(ID, out var str)
                ? str
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringID(ulong id) => ID = id;

        public readonly bool IsValid => ID != 0;
        public override bool Equals(object obj) => obj is StringID ID && Equals(ID);

        public static bool operator ==(StringID a, StringID b) => a.ID == b.ID;

        public static bool operator !=(StringID a, StringID b) => a.ID != b.ID;

        public readonly override int GetHashCode() => ID.GetHashCode();

        public readonly bool Equals(StringID other) => other.ID == ID;
    }
}
