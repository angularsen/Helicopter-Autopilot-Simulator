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
using Microsoft.Xna.Framework.Input;
using Sim.Environment;

namespace Sim.Interface
{
    public class WorldScreen : Screen
    {
        public enum Mode
        {
            Normal,     // objects are selected on LClick; moved on right
            Build,      // objects are placed on LClick
        };

        private World world;    // reference to world, as the world screen obviously interacts with it frequently

        private Mode mode = Mode.Normal;
        private MouseHandler lClick;
        private MouseHandler rClick;

        private Label timeLabel;
        private List<ControlGroup> cgList = new List<ControlGroup>();
        private ControlGroup menu = new ControlGroup();     // menu components
        private ControlGroup gc = new ControlGroup();       // graphics components
        private ControlGroup wc = new ControlGroup();       // weather components
        private ControlGroup cc = new ControlGroup();       // camera controls
        private ControlGroup ac = new ControlGroup();       // agent controls
        private ControlGroup activeGroup;                   // currently active control group
        private Rectangle menuArea;

        private ITargetable selected;
        public event EventHandler SelectionInfoChanged;

        public ITargetable Selected { get { return selected; } set { selected = value; } }

        public WorldScreen(ScreenManager manager)
            : base(manager)
        {
            Initialize();
            this.world = manager.UI.Sim.World;
        }

        public override void Initialize()
        {

        }

        public override void LoadContent()
        {
            Viewport vp = manager.UI.GraphicsDevice.Viewport;

            cgList.Add(menu);
            cgList.Add(gc);
            cgList.Add(wc);
            cgList.Add(cc);
            cgList.Add(ac);

            SetupMenu(vp);
            SetupGC(vp);
            SetupWC(vp);
            SetupCC(vp);
            SetupAC(vp);

            lClick = new MouseHandler(OnLClick);
            rClick = new MouseHandler(OnRClick);
        }
        
        public override void ConnectControls()
        {
            manager.UI.Mouse.LRelease += lClick;
            manager.UI.Mouse.RRelease += rClick;

            foreach (ControlGroup cg in cgList)
                if (cg.Enabled)
                    cg.Connect(manager.UI);
        }

        public override void DisconnectControls()
        {
            manager.UI.Mouse.LRelease -= lClick;
            manager.UI.Mouse.RRelease -= rClick;

            foreach (ControlGroup cg in cgList)
                if (cg.Enabled)
                    cg.Disconnect(manager.UI);
        }

        public void SetMode(Mode mode)
        {
            this.mode = mode;
            world.Terrain.SetDrawCursor(mode == Mode.Build);
        }

        private void OnRClick(UIMouse mouse)
        {
            if (mode == Mode.Normal)
            {
                if (selected is LightBall)
                {
                    Vector3? cursorPos = world.Terrain.PickTerrain(manager.UI.MouseRay);
                    if (cursorPos.HasValue)
                    {
                        (selected as LightBall).SetWaypoints(world.Terrain.NodeGrid.Search(selected.Position, cursorPos.Value));
                    }
                }
            }
        }

        private void OnLClick(UIMouse mouse)
        {
            if (mode == Mode.Normal)
            {
                Select();
            }
            else if (mode == Mode.Build)
            {
                Vector3? pos = world.Terrain.PickTerrain(manager.UI.MouseRay);
                if (pos.HasValue)
                    manager.UI.Sim.World.Wildlife.AddBall(pos.Value + Vector3.UnitY * 20);
            }
        }

        private void Select()
        {
            Ray r = GMath.GetRayFromScreenPoint(manager.UI.Mouse.ScreenPositionV,
                manager.UI.GraphicsDevice.Viewport, manager.UI.Camera.Projection, manager.UI.Camera.View);
            r.Direction.Normalize();
            ITargetable selected = manager.UI.Sim.World.Wildlife.Select(r);

            if (selected != null)
            {
                if (this.selected != null)
                    this.selected.Updated -= UpdateSelectionInfo;

                this.selected = selected;
                this.selected.Updated += UpdateSelectionInfo;
                manager.UI.Camera.SetTrackingTarget(this.selected);
                SelectionInfoChanged(selected, EventArgs.Empty);
            }
            else
            {
                //this.selected = null;
                //SelectedChanged(null, EventArgs.Empty);
            }
        }

        private void UpdateMouseInput()
        {
            UIMouse m = manager.UI.Mouse;
            if (m.State.RightButton == ButtonState.Pressed)
            {
                if (m.LastMove != Vector2.Zero)
                    manager.UI.Camera.Rotate(new Vector2(m.LastMove.Y, m.LastMove.X) / -200.0f);
            }
        }

        private void UpdateKeyboardInput()
        {
            UIKeyboard keyboard = manager.UI.Keyboard;

            // Key Presses
            Keys[] pressedKeys = keyboard.State.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Length; i++)
            {
                switch (pressedKeys[i])
                {
                    case Keys.W:
                        manager.UI.Camera.MoveForward();
                        break;
                    case Keys.A:
                        manager.UI.Camera.MoveLeft();
                        break;
                    case Keys.S:
                        manager.UI.Camera.MoveBack();
                        break;
                    case Keys.D:
                        manager.UI.Camera.MoveRight();
                        break;
                    case Keys.Q:
                        manager.UI.Camera.RotateLeft();
                        break;
                    case Keys.E:
                        manager.UI.Camera.RotateRight();
                        break;
                    case Keys.R:
                        manager.UI.Camera.RotateDown();
                        break;
                    case Keys.F:
                        manager.UI.Camera.RotateUp();
                        break;
                    case Keys.Up:
                        manager.UI.Camera.MoveUp();
                        break;
                    case Keys.Down:
                        manager.UI.Camera.MoveDown();
                        break;
                }
            }

            // Key Pushes
            if (manager.UI.Keyboard.KeyPushed(Keys.Escape))
            {
                if (selected == null)
                    manager.ChangeScreen(new MenuScreen(manager));
                else
                {
                    selected.Updated -= UpdateSelectionInfo;
                    selected = null;
                    SelectionInfoChanged(null, EventArgs.Empty);
                }
            }
            if (manager.UI.Keyboard.KeyPushed(Keys.B))
                SetMode(mode == Mode.Build ? Mode.Normal : Mode.Build);
            if (manager.UI.Keyboard.KeyPushed(Keys.X))
                Sim.Settings.Graphics.Default.ShowBBs = !Sim.Settings.Graphics.Default.ShowBBs;
            if (manager.UI.Keyboard.KeyPushed(Keys.P))
                world.Paused = !world.Paused;
        }

        private void UpdateSelectionInfo(object src, EventArgs args)
        {
            SelectionInfoChanged(src, args);
        }

        #region Screen Overrides

        protected override void OnTransparencyChange()
        {
            foreach (ControlGroup cg in cgList)
                cg.OnTransparencyChange(transparency);
        }

        public override void Update(GameTime time)
        {
            if (!manager.UI.Console.Open)
                UpdateKeyboardInput();

            UpdateMouseInput();

            if (timeLabel != null)
                timeLabel.SetText(manager.UI.Sim.World.ClockTime, Label.Fit.AlignCenter);
        }

        public override void Draw(SpriteBatch sb)
        {
            foreach (ControlGroup cg in cgList)
                cg.Draw(sb);
        }

        #endregion

        #region Setup

        private void SetupMenu(Viewport vp)
        {
            menu.Clear();

            int menuHeight = vp.Height / 20;
            menuArea = new Rectangle(0, vp.Height - menuHeight, (int)(vp.Width * 0.7f), menuHeight);
            ControlMat menuSpace = new ControlMat(menuArea, 1, 5, 2, ControlMat.FillOrder.ColumnsFirst);
            int i = 0;

            Panel mPanel = new Panel("Menu Panel", new Rectangle(0, menuArea.Y, vp.Width, menuHeight), new Color(0, 0, 0, 128));
            mPanel.Texture = UserInterface.TexButtonM;
            menu.Add(mPanel);
            menu.Add(new Button("Menu", menuSpace.Spaces[i++], delegate() { manager.ChangeScreen(new MenuScreen(manager)); }));
            menu.Add(new Button("Drawing", menuSpace.Spaces[i++], delegate() { ChangeActiveCG(gc); }));
            menu.Add(new Button("Wildlife", menuSpace.Spaces[i++], delegate() { ChangeActiveCG(ac); }));
            menu.Add(new Button("Weather", menuSpace.Spaces[i++], delegate() { ChangeActiveCG(wc); }));
            menu.Add(new Button("Camera", menuSpace.Spaces[i++], delegate() { ChangeActiveCG(cc); }));
            timeLabel = new Label("Time", new Rectangle(menuSpace.Spaces[i - 1].Right, menuArea.Y, vp.Width - menuArea.Width, menuArea.Height), Label.Fit.AlignCenter);
            menu.Add(timeLabel);
        }

        /// <summary>
        /// Sets the active control group from the menu groups
        /// </summary>
        private void ChangeActiveCG(ControlGroup toOpen)
        {
            if (activeGroup != null && activeGroup != toOpen)
                activeGroup.Disable(manager.UI);
            activeGroup = toOpen;
            activeGroup.ToggleEnabled(manager.UI);
        }

        private void SetupGC(Viewport vp)
        {
            gc.Clear();

            
            Sim.Environment.World world = manager.UI.Sim.World;
            Rectangle gcArea = new Rectangle(0, 0, vp.Width / 3, (int)(vp.Height / 1.5f));
            ControlMat gcSpace = new ControlMat(gcArea, 16, 1, 2, ControlMat.FillOrder.RowsFirst);
            int i = 0;

            gc.Add(new Label("Drawing Options", gcSpace.Spaces[i++], Label.Fit.AlignCenter));

            Spinner<FillMode> gcFillSpinner = new Spinner<FillMode>("Fill Mode", gcSpace.Spaces[i++]);
            gcFillSpinner.Add(FillMode.Solid);
            gcFillSpinner.Add(FillMode.WireFrame);
            gcFillSpinner.Add(FillMode.Point);
            gcFillSpinner.Use += delegate() { world.FillMode = gcFillSpinner.GetSelected(); };
            gc.Add(gcFillSpinner);
            gc.Disable(manager.UI);

            Sim.Settings.Graphics gs = Sim.Settings.Graphics.Default;

            gc.Add(new Button("Sky", gcSpace.Spaces[i++], delegate() { world.Sky.Visible = !world.Sky.Visible; }));
            gc.Add(new Button("Terrain", gcSpace.Spaces[i++], delegate() { world.Terrain.Visible = !world.Terrain.Visible; }));
            gc.Add(new Button("Water", gcSpace.Spaces[i++], delegate() { world.Water.Visible = !world.Water.Visible; }));
            gc.Add(new Button("Axes", gcSpace.Spaces[i++], delegate() { world.DrawAxes = !world.DrawAxes; }));
            gc.Add(new Button("Vegetation", gcSpace.Spaces[i++], delegate() { world.Vegetation.Visible = !world.Vegetation.Visible; }));
            gc.Add(new Button("AI Nodes", gcSpace.Spaces[i++], delegate() { world.Terrain.DrawNodes = !world.Terrain.DrawNodes; }));
            gc.Add(new Button("Terrain Normals", gcSpace.Spaces[i++], delegate() { world.Terrain.DrawNormals = !world.Terrain.DrawNormals; }));
            gc.Add(new Button("Detail Texturing", gcSpace.Spaces[i++], delegate() { 
                gs.TerrainDetail = !gs.TerrainDetail; 
                world.Terrain.Effect.Parameters["bDetailEnabled"].SetValue(gs.TerrainDetail); }));
            gc.Add(new Button("Bump Mapping", gcSpace.Spaces[i++], delegate() { 
                gs.BumpEnabled = !gs.BumpEnabled;
                world.Terrain.Effect.Parameters["bBumpEnabled"].SetValue(gs.BumpEnabled);}));
            gc.Add(new Button("Shadow Mapping", gcSpace.Spaces[i++], delegate() {
                gs.ShadowsEnabled = !gs.ShadowsEnabled;
                Shaders.Common.Parameters["bShadowsEnabled"].SetValue(gs.ShadowsEnabled);}));
            gc.Add(new Button("Bloom", gcSpace.Spaces[i++], delegate()
            {
                gs.BloomEnabled = !gs.BloomEnabled;
            }));

            Slider biSlider = new Slider("Bloom Intes.", gcSpace.Spaces[i++],0,10);
            biSlider.SetValue(1.25f);
            biSlider.Use += delegate(float v) { manager.UI.Sim.PostProcessManager.Bloom.SetIntensity(v);};
            gc.Add(biSlider);

            Slider btSlider = new Slider("Bloom Thres.", gcSpace.Spaces[i++], 0, 1);
            btSlider.SetValue(0.6f);
            btSlider.Use += delegate(float v) { manager.UI.Sim.PostProcessManager.Bloom.SetThreshold(v); };
            gc.Add(btSlider);

            Slider bbSlider = new Slider("Bloom Blur", gcSpace.Spaces[i++], 0, 20);
            bbSlider.SetValue(8);
            bbSlider.Use += delegate(float v) { manager.UI.Sim.PostProcessManager.Bloom.SetBlur(v); };
            gc.Add(bbSlider);
        }

        private void SetupWC(Viewport vp)
        {
            wc.Clear();
            Sim.Environment.World world = manager.UI.Sim.World;
            wc.Disable(manager.UI);
            Rectangle wcArea = new Rectangle(0, 0, vp.Width / 2, vp.Height / 3);
            ControlMat wcSpace = new ControlMat(wcArea, 7, 1, 2, ControlMat.FillOrder.RowsFirst);
            int i = 0;

            wc.Add(new Label("Weather Options", wcSpace.Spaces[i++], Label.Fit.AlignCenter));

            ToggleButton rain = new ToggleButton("Active", wcSpace.Spaces[i++]);
            rain.Use += delegate() { world.Weather.Active = !world.Weather.Active; };
            wc.Add(rain);

            Slider turbSlider = new Slider("Turbulence", wcSpace.Spaces[i++], 0, 30);
            turbSlider.SetValue(Sim.Settings.Graphics.Default.WindTurbulence);
            turbSlider.Use += delegate(float v) { Sim.Settings.Graphics.Default.WindTurbulence = v; };
            wc.Add(turbSlider);

            Slider windX = new Slider("Wind X", wcSpace.Spaces[i++], -75.0f, 75.0f);
            windX.SetValue(Sim.Settings.Graphics.Default.WindX);
            windX.Use += delegate(float v) { Sim.Settings.Graphics.Default.WindX = v; };
            wc.Add(windX);

            Slider windZ = new Slider("Wind Z", wcSpace.Spaces[i++], -75.0f, 75.0f);
            windZ.SetValue(Sim.Settings.Graphics.Default.WindZ);
            windZ.Use += delegate(float v) { Sim.Settings.Graphics.Default.WindZ = v; };
            wc.Add(windZ);

            Slider overcast = new Slider("Overcast", wcSpace.Spaces[i++], 1, 5);
            overcast.SetValue(Sim.Settings.Graphics.Default.Overcast);
            overcast.Use += delegate(float v) { Sim.Settings.Graphics.Default.Overcast = v; Shaders.Common.Parameters["overcast"].SetValue(v); };
            wc.Add(overcast);
        }

        private void SetupCC(Viewport vp)
        {
            Camera c = manager.UI.Camera;
            Rectangle ccArea = new Rectangle(0, 0, (int)(vp.Width * 0.5f), vp.Height / 3);
            ControlMat ccSpace = new ControlMat(ccArea, 7, 1, 2, ControlMat.FillOrder.RowsFirst);

            cc.Add(new Label("Camera Options", ccSpace.Spaces[0], Label.Fit.AlignCenter));

            Spinner<Camera.Mode> mode = new Spinner<Camera.Mode>("Mode", ccSpace.Spaces[1]);
            foreach (Camera.Mode cMode in Enum.GetValues(typeof(Camera.Mode)))
                mode.Add(cMode);
            mode.Use += delegate() { manager.UI.Camera.SetMode(mode.GetSelected()); };
            c.Changed += delegate(object src, EventArgs e) { mode.Select(c.CurrentMode); };
            cc.Add(mode);

            Slider transSpeed = new Slider("TSpeed", ccSpace.Spaces[2], 0.1f, 5.0f);
            transSpeed.SetValue(manager.UI.Camera.TranslateSpeed);
            transSpeed.Use += delegate(float v) { manager.UI.Camera.TranslateSpeed = v; };
            c.Changed += delegate(object src, EventArgs e) { transSpeed.SetValue(c.TranslateSpeed); };
            cc.Add(transSpeed);

            Slider rotSpeed = new Slider("RSpeed", ccSpace.Spaces[3], 0.02f, 0.1f);
            rotSpeed.SetValue(manager.UI.Camera.RotateSpeed);
            rotSpeed.Use += delegate(float v) { manager.UI.Camera.RotateSpeed = v; };
            c.Changed += delegate(object src, EventArgs e) { rotSpeed.SetValue(c.RotateSpeed); };
            cc.Add(rotSpeed);

            Label posInfo = new Label("Position: " + manager.UI.Camera.Position, ccSpace.Spaces[4], Label.Fit.AlignCenter);
            cc.Add(posInfo);

            Label rotInfo = new Label("Rotation: " + manager.UI.Camera.Rotation, ccSpace.Spaces[5], Label.Fit.AlignLeft);
            cc.Add(rotInfo);

            c.Changed += delegate(object src, EventArgs e)
            {
                posInfo.SetText("Position: " + c.Position, Label.Fit.AlignLeft);
                rotInfo.SetText("Rotation: " + c.Rotation, Label.Fit.AlignLeft);
            };

            cc.Disable(manager.UI);
        }

        private void SetupAC(Viewport vp)
        {
            Rectangle area = new Rectangle(0, 0, vp.Width / 3, vp.Height / 3);
            ControlMat mat = new ControlMat(area, 10, 1, 2, ControlMat.FillOrder.RowsFirst);
            Label selectionLabel = new Label("Selected Target", mat.Spaces[0], Label.Fit.AlignLeft);
            Label nameLabel = new Label("Name: ", mat.Spaces[1], Label.Fit.AlignLeft);
            Label positionLabel = new Label("Position: ", mat.Spaces[2], Label.Fit.AlignLeft);
            Label infoLabel = new Label("Info: ", mat.Spaces[3], Label.Fit.AlignLeft);

            SelectionInfoChanged += delegate(object src, EventArgs e)
            {
                nameLabel.Text = src == null ? "Name:" : "Name: " + selected.Name;
                positionLabel.Text = src == null ? "Position:" : "Position: " + selected.Position;
                infoLabel.Text = src == null ? "Info:" : "Info: " + selected.Info;
            };

            Slider separationSlider = new Slider("Separation", mat.Spaces[4], 0.0f, 1.0f);
            Slider cohesionSlider = new Slider("Cohesion", mat.Spaces[5], 0.0f, 1.0f);
            Slider wanderSlider = new Slider("Wander", mat.Spaces[6], 0.0f, 1.0f);
            Slider alignmentSlider = new Slider("Alignment", mat.Spaces[7], 0.0f, 1.0f);
            //alignmentSlider.Use += delegate(float v) { world.Wildlife.ApplySteerParameters(

            ac.Add(selectionLabel);
            ac.Add(nameLabel);
            ac.Add(positionLabel);
            ac.Add(infoLabel);
            ac.Disable(manager.UI);
        }

        #endregion
    }
}
