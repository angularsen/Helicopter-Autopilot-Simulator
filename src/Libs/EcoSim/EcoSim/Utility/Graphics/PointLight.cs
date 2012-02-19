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

/*
 * Note: this is based off of the XNA creator's club lighting sample series:
 * http://creators.xna.com/en-US/sample/shader_series5
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Utility
{
    public class PointLight
    {
        private Vector3 position;
        private Vector3 color;

        private EffectParameter lightParameter;
        private EffectParameter positionParameter;
        private EffectParameter rangeParameter;
        private EffectParameter colorParameter;

        public Vector3 Position { get { return position; } }
        public Vector3 Color { get { return color; } }
        
        public PointLight(Vector3 position, Vector3 color, EffectParameter lightParameter)
        {
            this.position = position;
            this.color = color;
            this.lightParameter = lightParameter;

            positionParameter = lightParameter.StructureMembers["vPosition"];
            rangeParameter = lightParameter.StructureMembers["fRange"];
            colorParameter = lightParameter.StructureMembers["vColor"];

            lightParameter.StructureMembers["fFalloff"].SetValue(2.0f);
            positionParameter.SetValue(position);
            positionParameter.SetValue(position);
            rangeParameter.SetValue(100);
            colorParameter.SetValue(color);
        }

        public void MoveTo(Vector3 position)
        {
            this.position = position;
            positionParameter.SetValue(position);
        }

        public void Move(Vector3 movement)
        {
            position += movement;
            positionParameter.SetValue(position);
        }

        public void ChangeRadius(float amount)
        {
            rangeParameter.SetValue(amount + rangeParameter.GetValueSingle());
        }
    }
}
