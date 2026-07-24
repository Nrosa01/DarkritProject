// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Darkrit.Math;

public struct Transform2D
{
    // Affin transform
    Vector2 BasisX, BasisY, Translation;
}
