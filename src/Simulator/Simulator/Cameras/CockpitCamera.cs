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

#if !XBOX
using System;
using Simulator.Tracking;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simulator.Interfaces;
using Simulator.StaticMeshes;

#endregion

namespace Simulator.Cameras
{
    public class CockpitCamera : CameraBase
    {
        private readonly Vector3 _cameraOffset;

        /// <summary>
        /// This vector is constant, and is used to multiply with the camera orientation quaternion to get the forward (look at) vector.
        /// </summary>
        private readonly Vector3 _relativeLookAtBase;

        private readonly int _screenHeight;
        private readonly int _screenWidth;
#if !XBOX
        private readonly Tracker _tracker;
#endif


        private float _pitchAngle;
        private float _yawAngle;

        #region Constructors

        public CockpitCamera(ICameraTarget vehicle, Vector3 cameraOffset, int screenX, int screenY)
            : base(vehicle.Position + cameraOffset)
        {
            Speed = 5f;
            TurnSpeed = 10f;

            LookAtTarget = vehicle;

            _cameraOffset = cameraOffset;
            _screenWidth = screenX;
            _screenHeight = screenY;

            // Point it in the positive Z direction
            _relativeLookAtBase = Vector3.Backward;

            _yawAngle += MathHelper.ToRadians(180);

            LookAt = _relativeLookAtBase + Position;

#if !XBOX
            _tracker = new Tracker();
#endif

        }

        #endregion

        /// <summary>
        /// Radians per second?
        /// </summary>
        public float TurnSpeed { get; set; }

        /// <summary>
        /// Meters per second?
        /// </summary>
        public float Speed { get; set; }

        public SimpleModel CockpitMesh { get; set; }

        #region Overrides

        /// <summary>
        /// In this camera implementation, the LookAtTarget is the object we have mounted the
        /// camera onto/inside. The name is kept so for compatibility of other camera implementations.
        /// </summary>
        public ICameraTarget Vehicle
        {
            get { return LookAtTarget; }
        }


        public override void Update(GameTime time)
        {
#if !XBOX
            Quaternion localHeadRotation = _tracker.IsConnected
                                               ? GetHeadRotationFromTracker()
                                               : GetHeadRotationFromMouse();
#else
            Quaternion localHeadRotation = GetHeadRotationFromGamePad();
#endif


            Quaternion worldHeadRotation = Quaternion.Multiply(Quaternion.CreateFromRotationMatrix(Vehicle.Rotation),
                                                               localHeadRotation);

            CockpitMesh.Position = Vehicle.Position;
            CockpitMesh.Rotation = Vehicle.Rotation;

            Vector3 relativeLookAt = Vector3.Transform(Vector3.Forward, worldHeadRotation);

            Position = CockpitMesh.Position + Vector3.Transform(_cameraOffset, CockpitMesh.Rotation);
            LookAt = Position + relativeLookAt;
            Up = Matrix.CreateFromQuaternion(worldHeadRotation).Up;


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

        #endregion

        #region Helpers

#if XBOX
        private Quaternion GetHeadRotationFromGamePad()
        {
            // Use left trigger to activate free look in cockpit
            if (GamePad.GetState(PlayerIndex.One).Triggers.Left > 0.5f)
            {
                Vector2 rightTS = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;

                _pitchAngle += MathHelper.ToRadians(rightTS.Y*TurnSpeed);
                _yawAngle -= MathHelper.ToRadians(rightTS.X*TurnSpeed);

                // Clamp the pitch angle to avoid rolling over the critical angles 90 and -90
                _pitchAngle = MathHelper.Clamp(_pitchAngle, MathHelper.ToRadians(-89.9f), MathHelper.ToRadians(89.9f));
            }

            Quaternion pitchRot = Quaternion.CreateFromAxisAngle(Vector3.Right, -_pitchAngle);
            Quaternion yawRot = Quaternion.CreateFromAxisAngle(Vector3.Up, _yawAngle);
            Quaternion headRotation = Quaternion.Concatenate(pitchRot, yawRot);

            return headRotation;
        }
#endif


#if !XBOX
        private Quaternion GetHeadRotationFromMouse()
        {
            int centerX = Convert.ToInt32(_screenWidth/2f);
            int centerY = Convert.ToInt32(_screenHeight/2f);

            MouseState mouse = Mouse.GetState();

            _pitchAngle += MathHelper.ToRadians((mouse.Y - centerY)*TurnSpeed*0.01f);
            _yawAngle -= MathHelper.ToRadians((mouse.X - centerX)*TurnSpeed*0.01f);


            // Clamp the pitch angle to avoid rolling over the critical angles 90 and -90
            _pitchAngle = MathHelper.Clamp(_pitchAngle, MathHelper.ToRadians(-89.9f), MathHelper.ToRadians(89.9f));

            // Reset the mouse position each time
            Mouse.SetPosition(centerX, centerY);

            Quaternion pitchRot = Quaternion.CreateFromAxisAngle(Vector3.Right, -_pitchAngle);
            Quaternion yawRot = Quaternion.CreateFromAxisAngle(Vector3.Up, _yawAngle);
            Quaternion headRotation = Quaternion.Concatenate(pitchRot, yawRot);

            return headRotation;
        }

        private Quaternion GetHeadRotationFromTracker()
        {
            // Transpose of vrpnToXNA
            var vrpnToXNAT = new Matrix(
                0, 0, -1, 0,
                1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, 0, 1);

            var vrpnToXNA = new Matrix(
                0, 1, 0, 0,
                0, 0, -1, 0,
                -1, 0, 0, 0,
                0, 0, 0, 1);

            // This is done in _tracker.IsConnected already
//            _tracker.Update();

            // The default HMD tracking setup sets "zero" position to be facing the HPC side of the lab.
            // We want "zero" HMD position to be facing either the windows or opposite of the windows
            // facing our computer screen and keyboard.
            // Z-axis is positive down so rotating around it equals yaw.
            Matrix faceTowardsWindows = Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
            Matrix faceOppositeWindows = Matrix.CreateRotationZ(MathHelper.ToRadians(+90));

            Matrix hmdRotVRPN = Matrix.CreateFromQuaternion(_tracker.HMD.Orientation);
            hmdRotVRPN *= faceOppositeWindows;

            Matrix hmdRotXNA = vrpnToXNA*hmdRotVRPN*vrpnToXNAT;

            return Quaternion.CreateFromRotationMatrix(hmdRotXNA);
        }
#endif


        #endregion
    }
}