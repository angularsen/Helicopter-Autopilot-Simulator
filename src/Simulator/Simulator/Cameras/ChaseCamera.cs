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
using Simulator.Interfaces;

#endregion

namespace Simulator.Cameras
{
    public class ChaseCamera : CameraBase
    {
        public ChaseCamera(ICameraTarget target)
        {
            LookAtTarget = target;
        }

        public ChaseCamera() : this(null)
        {
        }

        #region Methods

        public bool IsElastic { get; set; }

        /// <summary>
        /// Forces camera to be at desired Position and to stop moving. The is useful
        /// when the chased object is first created or after it has been teleported.
        /// Failing to call this after a large change to the chased object's Position
        /// will result in the camera quickly flying across the world.
        /// </summary>
        public override void Reset()
        {
            UpdateWorldPositions();

            // Stop motion
            _velocity = Vector3.Zero;

            // Force desired Position
            Position = _desiredPosition;

            UpdateMatrices();
        }


        public override ICamera Clone()
        {
            var r = new ChaseCamera
                        {
                            AspectRatio = AspectRatio,
                            ChaseDirection = ChaseDirection,
                            ChasePosition = ChasePosition,
                            Damping = Damping,
                            DesiredPosition = DesiredPosition,
                            DesiredPositionOffset = DesiredPositionOffset,
                            FarPlane = FarPlane,
                            FieldOfView = FieldOfView,
                            Frustum = Frustum,
                            IsElastic = IsElastic,
                            LookAt = LookAt,
                            LookAtOffset = LookAtOffset,
                            Mass = Mass,
                            NearPlane = NearPlane,
                            Position = Position,
                            Projection = Projection,
                            Stiffness = Stiffness,
                            LookAtTarget = LookAtTarget,
                            Up = Up,
                            Velocity = Velocity,
                            View = View,
                            WoldViewProj = WoldViewProj,
                            World = World
                        };

            return r;
        }

        /// <summary>
        /// Animates the camera from its current Position towards the desired offset
        /// behind the chased object. The camera's animation is controlled by a simple
        /// physical spring attached to the camera and anchored to the desired Position.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            UpdateWorldPositions();

            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (IsElastic)
            {
                // Calculate spring force
                Vector3 stretch = Position - _desiredPosition;
                Vector3 force = -_stiffness*stretch - _damping*_velocity;

                // Apply acceleration
                Vector3 acceleration = force/_mass;
                _velocity += acceleration*elapsed;

                // Apply velocity
                Position += _velocity*elapsed;
            }
            else
            {
                Position = LookAtTarget.Position - Vector3.Normalize(LookAtTarget.CameraForward)*2.0f +
                           Vector3.Normalize(LookAtTarget.CameraUp)*0.5f;
                LookAt = LookAtTarget.Position;
                Up = LookAtTarget.CameraUp;
            }

            UpdateMatrices();

            // Update projection and view matrices
            base.Update(gameTime);
        }

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            // Make sure we are using the most current camera chase target data
            // Variations may overriden in children implementations such as how
            // to follow a target.
            UpdateCameraChaseTarget();

            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            _desiredPosition = ChasePosition +
                               Vector3.TransformNormal(DesiredPositionOffset, transform);
            LookAt = ChasePosition +
                     Vector3.TransformNormal(LookAtOffset, transform);
        }

//        /// <summary>
//        /// Rebuilds camera's view and projection matricies.
//        /// </summary>
//        private new void UpdateMatrices()
//        {
//            View = Matrix.CreateLookAt(Position, LookAt, Up);
//            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
//        }

        #endregion

        /// <summary>
        /// Override this to update camera chase target in variations of ChaseCamera
        /// </summary>
        public virtual void UpdateCameraChaseTarget()
        {
            if (LookAtTarget != null)
            {
                ChasePosition = LookAtTarget.Position;
                ChaseDirection = LookAtTarget.CameraForward;
            }
        }

        #region Chased object properties (set externally each frame)

        /// <summary>
        /// Position of object being chased.
        /// </summary>
        public Vector3 ChasePosition { get; set; }

        /// <summary>
        /// Direction the chased object is facing.
        /// </summary>
        public Vector3 ChaseDirection { get; set; }

        #endregion

        #region Desired camera positioning (set when creating camera or changing view)

        private Vector3 _desiredPosition;
        private Vector3 _desiredPositionOffset = new Vector3(0, 2.0f, 2.0f);
        private Vector3 _lookAtOffset = new Vector3(0, 2.8f, 0);

        /// <summary>
        /// Desired camera Position in the chased object's coordinate system.
        /// </summary>
        public Vector3 DesiredPositionOffset
        {
            get { return _desiredPositionOffset; }
            set { _desiredPositionOffset = value; }
        }

        /// <summary>
        /// Desired camera Position in world space.
        /// </summary>
        public Vector3 DesiredPosition
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();

                return _desiredPosition;
            }
            private set { _desiredPosition = value; }
        }

        /// <summary>
        /// Look at point in the chased object's coordinate system.
        /// </summary>
        public Vector3 LookAtOffset
        {
            get { return _lookAtOffset; }
            set { _lookAtOffset = value; }
        }

//        /// <summary>
//        /// Look at point in world space.
//        /// </summary>
//        public Vector3 LookAt
//        {
//            get
//            {
//                // Ensure correct value even if update has not been called this frame
//                UpdateWorldPositions();
//
//                return _lookAt;
//            }
//        }

        #endregion

        #region Camera physics (typically set when creating camera)

        private float _damping = 600.0f;
        private float _mass = 50.0f;
        private float _stiffness = 1800.0f;

        /// <summary>
        /// Physics coefficient which controls the influence of the camera's Position
        /// over the spring force. The stiffer the spring, the closer it will stay to
        /// the chased object.
        /// </summary>
        public float Stiffness
        {
            get { return _stiffness; }
            set { _stiffness = value; }
        }

        /// <summary>
        /// Physics coefficient which approximates internal friction of the spring.
        /// Sufficient damping will prevent the spring from oscillating infinitely.
        /// </summary>
        public float Damping
        {
            get { return _damping; }
            set { _damping = value; }
        }

        /// <summary>
        /// Mass of the camera body. Heaver objects require stiffer springs with less
        /// damping to move at the same rate as lighter objects.
        /// </summary>
        public float Mass
        {
            get { return _mass; }
            set { _mass = value; }
        }

        #endregion

        #region Current camera properties (updated by camera physics)

        private Vector3 _velocity;

        /// <summary>
        /// Velocity of camera.
        /// </summary>
        public Vector3 Velocity
        {
            get { return _velocity; }
            private set { _velocity = value; }
        }

        #endregion
    }
}