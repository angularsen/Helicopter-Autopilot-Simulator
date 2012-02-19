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
    /// <summary>
    /// Used to render the game world
    /// </summary>
    public class Camera
    {
        public class MoveArgs : EventArgs
        {
            public Vector3 translation;
            public MoveArgs(Vector3 translation) { this.translation = translation; }
        }

        public event EventHandler Changed;
        public event EventHandler Moved;

        private GraphicsDevice gDevice;
        private Vector3 position = new Vector3(1000, 180, 1500);
        private Vector3 forward; 
        private Vector3 right; 
        private Vector3 up;
        private ITargetable trackingTarget;
        private Vector3 focusPoint;
        private Vector2 rotation = new Vector2(-0.03f, 0.4f);
        private BoundingFrustum frustum;

        private Matrix view;
        private Matrix projection;
        private Matrix rotMatrix;

        public float TranslateSpeed = 5.0f;
        public float RotateSpeed = 0.02f;
        private Mode currentMode = Mode.Standard;

        private EffectParameter pView;
        private EffectParameter pProjection;
        private EffectParameter pViewProjection;
        private EffectParameter pPosition;

        public enum Mode
        {
            Standard,
            Orbit,
            Track,
        };

        public Vector3 Position { get { return position; } }
        public Vector2 Rotation { get { return rotation; } }
        public Matrix View { get { return view; } }
        public Matrix Projection { get { return projection; } }
        public Mode CurrentMode { get { return currentMode; } }
        public BoundingFrustum Frustum { get { return frustum; } }

        public Camera(SimEngine sim)
        {
            this.gDevice = sim.GraphicsDevice;

            pView = Shaders.Common.Parameters["matView"];
            pProjection = Shaders.Common.Parameters["matProjection"];
            pViewProjection = Shaders.Common.Parameters["matViewProjection"];
            pPosition = Shaders.Common.Parameters["vCamPos"];

            UpdateProjectionMatrix();
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void Update(GameTime time)
        {
            switch (currentMode)
            {
                case Mode.Standard:
                    break;
                case Mode.Orbit:
                    MoveRight();
                    Focus(focusPoint);
                    break;
                case Mode.Track:
                    if (trackingTarget != null)
                        Focus(trackingTarget.Position);
                    break;
            }
        }

        private void OnCameraChange()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);

            Matrix viewProjection = view * projection;
            frustum = new BoundingFrustum(viewProjection);

            Shaders.Primitive.Projection = projection;
            Shaders.Primitive.View = view;
            pView.SetValue(view);
            pProjection.SetValue(projection);
            pViewProjection.SetValue(viewProjection);
            pPosition.SetValue(position);
        }

        private void OnCameraMove(Vector3 translation)
        {
            if (Moved != null)
                Moved(this, new MoveArgs(translation));
        }

        /// <summary>
        /// Sets the camera's target used in tracking mode
        /// </summary>
        public void SetTrackingTarget(ITargetable target)
        {
            trackingTarget = target;
        }

        /// <summary>
        /// Focuses the camera on a position
        /// </summary>
        public void Focus(Vector3 focus)
        {
            focusPoint = focus;
            Vector3 eye = focus - position;
            right = Vector3.Normalize(Vector3.Cross(eye, Vector3.Up));
            up = Vector3.Normalize(Vector3.Cross(right, eye));
            view = Matrix.CreateLookAt(position, focus, up);
            forward = Vector3.Normalize(eye);
            Vector3 xzDir = Vector3.Normalize(new Vector3(forward.X, 0, forward.Z));

            Vector2 rotations = new Vector2((float)Math.Acos(Vector3.Dot(xzDir, forward)),
                (float)Math.Acos(Vector3.Dot(xzDir, -Vector3.UnitZ)));
            rotation.Y = forward.X > 0 ? -rotations.Y : rotations.Y;
            rotation.X = forward.Y < 0 ? -rotations.X : rotations.X;

            OnCameraChange();
        }

        public void UpdateProjectionMatrix()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                gDevice.Viewport.AspectRatio, 1.0f, Sim.Settings.Graphics.Default.ViewDistance);
        }

        private void UpdateViewMatrix()
        {
            view = Matrix.CreateLookAt(position, position + forward, up);
            OnCameraChange();
        }

        private void UpdateRotationMatrix()
        {
            rotMatrix = Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y);
            forward = Vector3.Transform(Vector3.Forward, rotMatrix);
            right = Vector3.Transform(Vector3.Right, rotMatrix);
            up = Vector3.Transform(Vector3.Up, rotMatrix);
        }

        #region Movement

        public void SetMode(Mode mode)
        {
            if (currentMode != mode) {
                currentMode = mode;
                OnCameraChange();
            }
        }

        public void SetPosition(Vector3 pos)
        {
            OnCameraMove(position - pos);
            position = pos;
            UpdateViewMatrix();
        }

        public void Rotate(Vector2 rot)
        {
            rotation.X += rot.X;
            rotation.Y += rot.Y;
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void RotateUp()
        {
            rotation.X += RotateSpeed;
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void RotateDown()
        {
            rotation.X -= RotateSpeed;
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void RotateRight()
        {
            rotation.Y -= RotateSpeed;
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void RotateLeft()
        {
            rotation.Y += RotateSpeed;
            UpdateRotationMatrix();
            UpdateViewMatrix();
        }

        public void MoveRight()
        {
            Move(right * TranslateSpeed);
        }

        public void MoveLeft()
        {
            Move(-right * TranslateSpeed);
        }

        public void MoveForward()
        {
            Move(forward * TranslateSpeed);
        }

        public void MoveBack()
        {
            Move(-forward * TranslateSpeed);
        }

        public void MoveUp()
        {
            Move(up * TranslateSpeed);
        }

        public void MoveDown()
        {
            Move(-up * TranslateSpeed);
        }

        public void Move(Vector3 translation)
        {
            position += translation;
            UpdateViewMatrix();
            OnCameraMove(translation);
        }

        #endregion

        public override string ToString()
        {
            String pString = String.Format("Pos: X:{0:F} Y:{1:F} Z:{2:F} ", new object[] { position.X, position.Y, position.Z });
            String rString = String.Format("Rot: X:{0:F} Y:{1:F}", rotation.X, rotation.Y);
            return pString + rString;
        }
    }
}
