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
    class Spinner<T> : Control
    {
        public event ButtonHandler Use;

        private Label itemLabel;
        private Button prevButton;
        private Button nextButton;
        private List<T> items;
        private int index;

        public Spinner(String name, Rectangle area)
            : base(name, area)
        {
            items = new List<T>();

            itemLabel = new Label(String.Empty, new Rectangle(area.X + area.Height - 5, area.Y, area.Width - 2 * 
                area.Height + 10, area.Height), Label.Fit.AlignCenter);
            itemLabel.DrawBackground = true;
            prevButton = new Button("<", new Rectangle(area.X, area.Y, area.Height, area.Height));
            prevButton.Use += SelectPrevItem;
            nextButton = new Button(">", new Rectangle(area.Right - area.Height, area.Y, area.Height, area.Height));
            nextButton.Use += SelectNextItem;
        }

        public void Add(T item)
        {
            if (items.Count == 0)
                itemLabel.SetText(item.ToString(), Label.Fit.AlignCenter);
            items.Add(item);
        }

        public T GetSelected()
        {
            return items[index];
        }

        public void Select(T item)
        {
            index = items.IndexOf(item);
            Update();
        }

        private void SelectNextItem()
        {
            index = index == items.Count - 1 ? 0 : ++index;
            Update();
        }

        private void SelectPrevItem()
        {
            index = index == 0 ? items.Count - 1 : --index;
            Update();
        }

        private void Update()
        {
            itemLabel.SetText(items[index].ToString(), Label.Fit.AlignCenter);
            if (Use != null)
                Use();
        }

        public override void Draw(SpriteBatch sb)
        {
            itemLabel.Draw(sb);
            prevButton.Draw(sb);
            nextButton.Draw(sb);
        }

        #region Event Logic

        public override void Register(UIMouse mouse)
        {
            base.Register(mouse);
            prevButton.Register(mouse);
            nextButton.Register(mouse);
            itemLabel.Register(mouse);
        }

        public override void Unregister(UIMouse mouse)
        {
            base.Unregister(mouse);
            prevButton.Unregister(mouse);
            nextButton.Unregister(mouse);
            itemLabel.Unregister(mouse);
        }

        public override void OnTransparencyChange(float totalTransparency)
        {
            itemLabel.OnTransparencyChange(totalTransparency);
            nextButton.OnTransparencyChange(totalTransparency);
            prevButton.OnTransparencyChange(totalTransparency);
        }

        protected override void OnAreaChange() { }

        protected override void OnMouseLeftPress(UIMouse mouse) { }

        protected override void OnMouseLeftRelease(UIMouse mouse) { }

        protected override void OnMouseRightPress(UIMouse mouse) { }

        protected override void OnMouseRightRelease(UIMouse mouse) { }

        #endregion
    }
}
