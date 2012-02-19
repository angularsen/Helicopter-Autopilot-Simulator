/* 
 * Environment Simulator
 * Copyright (C) 2008-2009 Justin Stoecker
 * 
 * This program is free software; you can redistribute it and/or modify it under the terms of the 
 * GNU General Public License as published by the Free Software Foundation; either version 2 of 
 * the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sim
{
    /// <summary>
    /// Graphics algorithms
    /// </summary>
    public static class GMath
    {
        public static Ray GetRayFromScreenPoint(Vector2 screenPosition, Viewport vp, Matrix pMatrix, Matrix vMatrix)
        {
            // unproject screen coordinates to world coordinates
            Vector3 near = vp.Unproject(new Vector3(screenPosition.X, screenPosition.Y, vp.MinDepth),
                pMatrix, vMatrix, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(screenPosition.X, screenPosition.Y, vp.MaxDepth),
                pMatrix, vMatrix, Matrix.Identity);

            // get the direction of the ray from near vector to far vector
            Vector3 direction = far - near;

            return new Ray(near, direction);
        }

        /// <summary>
        /// Returns the angle of incline of the vector above the horizontal plane y=0
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float AngleOfIncline(Vector3 v)
        {
            if (v == Vector3.Up)
                return 90;
            return MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(v, new Vector3(v.X, 0, v.Z))));
        }

    }
}
