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

namespace Sim.Interface
{
    class ToggleButton : Button
    {
        public Boolean ToggledOn;

        public ToggleButton(String name, Rectangle area) 
            : base(name, area)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            if (down)
                base.DrawThreeParts(sb, colorDown);
            else if (hovering)
                base.DrawThreeParts(sb, colorHover);
            else
                base.DrawThreeParts(sb, colorUp);

            if (active)
                label.TextColor = hovering ? Color.White : Color.LightGray;
            label.Draw(sb);
        }

        #region Event Logic

        protected override void OnMouseLeftRelease(UIMouse mouse)
        {
            if (hovering)
            {
                ToggledOn = !ToggledOn;
                if (!ToggledOn)
                    down = false;
                OnUse();
            }
        }

        #endregion
    }
}
