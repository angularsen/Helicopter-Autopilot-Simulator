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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sim
{
    public class PostProcessManager
    {
        private GraphicsDevice g;
        private SpriteBatch sb;
        private Rectangle screen;

        private Bloom bloom;

        public Bloom Bloom { get { return bloom; } }
        public Rectangle Screen { get { return screen; } }

        public void LoadContent(GraphicsDevice g, ContentManager cm)
        {
            this.g = g;
            PresentationParameters pp = g.PresentationParameters;
            screen = new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

            bloom = new Bloom(this);
            bloom.LoadContent(g, cm);

            sb = new SpriteBatch(g);
        }

        public void Process(RenderTarget2D sceneTarget)
        {
            bloom.Process(g, sceneTarget);
        }

        public void RenderScreenQuad(Effect effect, EffectTechnique technique, Texture2D texture)
        {
            effect.CurrentTechnique = technique;

            sb.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            effect.Begin();
            technique.Passes[0].Begin();
            sb.Draw(texture, screen, Color.White);
            technique.Passes[0].End();
            effect.End();
            sb.End();
        }

        public void RenderScreenQuad(Texture2D texture)
        {
            sb.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            sb.Draw(texture, screen, Color.White);
            sb.End();
        }
    }
}
