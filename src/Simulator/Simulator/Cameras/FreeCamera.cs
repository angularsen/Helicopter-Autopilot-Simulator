#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simulator.Interfaces;

#endregion

namespace Simulator.Cameras
{
    public enum FreeCameraTypes
    {
        FirstPersonShooter,
        Spaceship
    }

    public class FreeCamera : CameraBase
    {
        /// <summary>
        /// This vector is constant, and is used to multiply with the camera orientation quaternion to get the forward (look at) vector.
        /// </summary>
        private readonly Vector3 _relativeLookAtBase;

        private readonly int _screenHeight;
        private readonly int _screenWidth;

        private float _pitchAngle;
        private float _yawAngle;

        #region Constructors

        public FreeCamera()
        {

        }

        public FreeCamera(Vector3 position, int screenX, int screenY)
            : base(position)
        {
            Speed = 500f;
            TurnSpeed = 0.1f;
            _screenWidth = screenX;
            _screenHeight = screenY;

            // Point it in the positive Z direction
            _relativeLookAtBase = Vector3.Backward;

            _yawAngle += MathHelper.ToRadians(180);

            LookAt = _relativeLookAtBase + Position;
            MovementType = FreeCameraTypes.Spaceship;
        }

        public FreeCamera(Vector3 position, int screenX, int screenY, float speed, float turnSpeed)
            : this(position, screenX, screenY)
        {
            Speed = speed;
            TurnSpeed = turnSpeed;
        }

        #endregion

        /// <summary>
        /// Choose how the free camera should behave.
        /// </summary>
        public FreeCameraTypes MovementType { get; set; }

        /// <summary>
        /// Radians per second?
        /// </summary>
        public float TurnSpeed { get; set; }

        /// <summary>
        /// Meters per second?
        /// </summary>
        public float Speed { get; set; }


        private Vector3 UpdateByGamePad(float dt)
        {
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            if (gamePad.IsConnected)
            {
                _pitchAngle -= gamePad.ThumbSticks.Right.Y*TurnSpeed;
                _yawAngle -= gamePad.ThumbSticks.Right.X*TurnSpeed;

                Vector3 moveVector = Vector3.Zero;

                moveVector += Vector3.Forward * (-gamePad.ThumbSticks.Left.Y);
                moveVector += Vector3.Right * (-gamePad.ThumbSticks.Left.X);

                // Move along the direction given speed and a time delta.
                moveVector *= /*speedBoost * */ Speed * dt;
                return moveVector;
            }

            return Vector3.Zero;
        }

        private Vector3 UpdateByKeyboardAndMouse(float dt)
        {
            int centerX = Convert.ToInt32(_screenWidth/2f);
            int centerY = Convert.ToInt32(_screenHeight/2f);

           
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            _pitchAngle += MathHelper.ToRadians((mouse.Y - centerY)*TurnSpeed); // pitch
            _yawAngle -= MathHelper.ToRadians((mouse.X - centerX)*TurnSpeed); // yaw

            // Reset the mouse position each time
            Mouse.SetPosition(centerX, centerY);


            Vector3 moveVector = Vector3.Zero;

            // Use keys to move along the look at vector (or strafe sideways perpendicular to it)
            if (keyboard.IsKeyDown(Keys.W))
            {
                // Backward is defined as positive Z (forward in LookAt direction)
                moveVector += Vector3.Backward;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                // Forward is defined as negative Z (backward in LookAt direction)
                moveVector += Vector3.Forward;
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                moveVector += Vector3.Right;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                moveVector += Vector3.Left;
            }
            if (keyboard.IsKeyDown(Keys.R))
            {
                moveVector += Vector3.Up;
            }
            if (keyboard.IsKeyDown(Keys.F))
            {
                moveVector += Vector3.Down;
            }

            float speedBoost = keyboard.IsKeyDown(Keys.LeftShift) ? 10 : 1;
            
            // Move along the direction given speed and a time delta.
            moveVector *= speedBoost * Speed * dt;

            return moveVector;
        }

        public override void Update(GameTime time)
        {
            var dt = (float) time.ElapsedGameTime.TotalSeconds;

            Vector3 moveVector = Vector3.Zero;
#if XBOX
            moveVector = UpdateByGamePad(dt);
#else
            moveVector = UpdateByKeyboardAndMouse(dt);
#endif
                        
            // Clamp the pitch angle to avoid rolling over the critical angles 90 and -90
            _pitchAngle = MathHelper.Clamp(_pitchAngle, MathHelper.ToRadians(-89.9f), MathHelper.ToRadians(89.9f));

            
            // Calculate view and FPS view matrices
            Matrix cameraViewRotationMatrix = Matrix.CreateRotationX(_pitchAngle)*Matrix.CreateRotationY(_yawAngle);
            Matrix cameraMoveRotationMatrix = Matrix.CreateRotationY(_yawAngle);
            Vector3 relativeLookAt = Vector3.Transform(_relativeLookAtBase, cameraViewRotationMatrix);

            // Move the camera based on input and view transformations
            if (MovementType == FreeCameraTypes.Spaceship)
            {
                Position += Vector3.Transform(moveVector, cameraViewRotationMatrix);
            }
            else if (MovementType == FreeCameraTypes.FirstPersonShooter)
            {
                Position += Vector3.Transform(moveVector, cameraMoveRotationMatrix);
            }
            else throw new NotImplementedException("MovementType == " + MovementType);

            LookAt = relativeLookAt + Position;

            // Update matrices using base class
            base.Update(time);
        }


        public override void Reset()
        {
        }

        public override ICamera Clone()
        {
            var r = new FreeCamera(Position, _screenWidth, _screenHeight)
                        {
                            //                Angle = Angle,
                            AspectRatio = AspectRatio,
                            FarPlane = FarPlane,
                            FieldOfView = FieldOfView,
                            Frustum = Frustum,
                            LookAt = LookAt,
                            NearPlane = NearPlane,
                            Projection = Projection,
                            Speed = Speed,
                            TurnSpeed = TurnSpeed,
                            LookAtTarget = LookAtTarget,
                            View = View,
                            WoldViewProj = WoldViewProj,
                            World = World
                        };

            return r;
        }
    }
}