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

using Microsoft.Xna.Framework;
using Simulator.Interfaces;

#endregion

namespace Simulator.Cameras
{
    /// <summary>
    /// Basic functionality shared between the different camera implementations,
    /// such as updating the view and projection matrices each loop cycle.
    /// </summary>
    public abstract class CameraBase : ICamera
    {
        private float _aspectRatio;
        private float _farPlane;
        private float _fieldOfView;
        private float _nearPlane;

        #region Constructors

        protected CameraBase()
        {
            // Some default property values, so the camera will work even
            // if the properties are not explicitly set by the client code
            _aspectRatio = 1280/1024f;
            _fieldOfView = MathHelper.ToRadians(45);
            _nearPlane = 1f;
            _farPlane = 1000f;
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
            
            Position = new Vector3(0, 1, -2);
            LookAt = Vector3.Zero;
            Up = Vector3.Up;

            World = Matrix.Identity;
//            View = Matrix.CreateLookAt(Position, LookAt, Up);
//            Frustum = new BoundingFrustum(View*Projection);

            // Update matrices from default properties
//            Update(new GameTime());
        }

        protected CameraBase(Vector3 position)
            : this(position, Vector3.Zero)
        {
        }

        protected CameraBase(Vector3 position, Vector3 lookAt) : this()
        {
            Position = position;
            LookAt = lookAt;
        }

        #endregion

        private void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }

        #region Propeties

        public Matrix World { get; set; }
        public Matrix WoldViewProj { get; set; }
        public Vector3 LookAt { get; set; }

        public float FarPlane
        {
            get { return _farPlane; }
            set
            {
                _farPlane = value;
                UpdateProjection();
            }
        }

        public float NearPlane
        {
            get { return _nearPlane; }
            set
            {
                _nearPlane = value;
                UpdateProjection();
            }
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                _aspectRatio = value;
                UpdateProjection();
            }
        }

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                UpdateProjection();
            }
        }

       

        public Vector3 Up { get; protected set; }

        public Vector3 Position { get; set; }

        public Matrix Projection { get; set; }

        public BoundingFrustum Frustum { get; set; }

        public ICameraTarget LookAtTarget { get; set; }

        public Matrix View { get; set; }

        #endregion

        #region Public methods

        public virtual void Update(GameTime time)
        {
            UpdateMatrices();
        }

        public virtual void UpdateMatrices()
        {
            View = GetView();

            Matrix viewProj = View*Projection;
            Frustum = new BoundingFrustum(viewProj);
            WoldViewProj = World*viewProj;
        }

        #endregion

        #region Abstract members

        public virtual void DrawHUD(GameTime time)
        {
            // We don't require all cameras to implement this, but they have the option to.
        }

        public abstract void Reset();

        public abstract ICamera Clone();

        /// <summary>
        /// Allow subclasses to specify the view matrix in a different manner than by Matrix.CreateLookAt().
        /// </summary>
        /// <returns></returns>
        protected virtual Matrix GetView()
        {
            return Matrix.CreateLookAt(Position, LookAt, Up);
        }

        #endregion
    }
}