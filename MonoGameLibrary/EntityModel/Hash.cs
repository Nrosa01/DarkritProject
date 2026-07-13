// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.EntityModel
{
    public static class Hash
    {
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
}
