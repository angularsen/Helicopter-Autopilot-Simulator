#region Copyright

// A�DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright � 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// Event args used in the ClosestPointPicker class to indicate
    /// when the closest point has changed.
    /// </summary>
    public class ClosestPointArgs : EventArgs
    {
        /// <summary>
        /// The closest point
        /// </summary>
        private readonly Point closestPoint;

        /// <summary>
        /// Whether the closest point in within the test distance or not
        /// </summary>
        private readonly bool locked;

        /// <summary>
        /// Constructor. Initializes all the fields.
        /// </summary>
        /// <param name="locked"></param>
        /// <param name="closestPoint"></param>
        public ClosestPointArgs(bool locked, Point closestPoint)
        {
            this.locked = locked;
            this.closestPoint = closestPoint;
        }

        public bool Locked
        {
            get { return locked; }
        }

        public Point ClosestPoint
        {
            get { return closestPoint; }
        }
    }

    /// <summary>
    /// This class picks the closest point to the screen point passed in. Typically
    /// used to find the closest scene point to the mouse pointer.
    /// </summary>
    internal class ClosestPointPicker
    {
        #region Private Fields

        /// <summary>
        /// A list of points to check against
        /// </summary>
        private readonly List<Point> points;

        /// <summary>
        /// The closest point to the cursor
        /// </summary>
        private Point closestPoint = new Point(0, 0);

        /// <summary>
        /// Flag indicating that the closest point is within the minimum range
        /// </summary>
        private bool locked = false;

        /// <summary>
        /// The minimum distance the cursor has to be away from the closest
        /// point to be locked
        /// </summary>
        private Rect minimumDistance;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        // Constructor. Initializes class fields.
        /// </summary>
        /// <param name="minimumDistance"></param>
        internal ClosestPointPicker(Size minimumDistance)
        {
            this.minimumDistance = new Rect(minimumDistance);
            points = new List<Point>();
        }

        /// <summary>
        /// Handles when the mouse moves
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="screenToSceneTransform"></param>
        public void MouseMoved(Point mousePos, GeneralTransform screenToSceneTransform)
        {
            if (screenToSceneTransform == null)
                return;
            // Convert the mouse coordinates to scene coordinates
            mousePos = screenToSceneTransform.Transform(mousePos);

            bool newLocked = false;
            Point newPoint = mousePos;

            if (points.Count > 0)
            {
                // Transform the minimum distance ignoring the translation
                Rect minimumBounds = screenToSceneTransform.TransformBounds(minimumDistance);

                double nearestDistanceSquared;
                newPoint = GetEllipseScaledNearestPoint(points, mousePos, (Vector) (minimumBounds.Size),
                                                        out nearestDistanceSquared);
                newLocked = nearestDistanceSquared <= 1;
            }

            bool lockedChanged = newLocked != locked;
            bool pointChanged = newPoint != closestPoint;

            locked = newLocked;
            closestPoint = newPoint;

            if ((pointChanged && locked) || lockedChanged)
            {
                OnClosestPointChanged();
            }
        }

        /// <summary>
        /// Gets the nearest point but allows for normalising the x and y
        /// by different distances, which is required if you scale the x
        /// and y seperately, but want to get the visually closest point.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="point"></param>
        /// <param name="ratio"></param>
        /// <param name="nearestDistanceSquared"></param>
        /// <returns></returns>
        public static Point GetEllipseScaledNearestPoint(IEnumerable<Point> points, Point point, Vector ratio,
                                                         out double nearestDistanceSquared)
        {
            var inverseNormalisation = new Point(1/ratio.X, 1/ratio.Y);

            nearestDistanceSquared = 0;

            Point nearestPoint = point;

            if (points == null)
                return point;

            // Just pick off the first point to initialize stuff
            foreach (Point testPoint in points)
            {
                nearestDistanceSquared =
                    new Vector((testPoint.X - point.X)*inverseNormalisation.X,
                               (testPoint.Y - point.Y)*inverseNormalisation.Y).LengthSquared;
                nearestPoint = testPoint;
                break;
            }

            // Loop through all points to find the closest
            foreach (Point testPoint in points)
            {
                double distanceSquared =
                    new Vector((testPoint.X - point.X)*inverseNormalisation.X,
                               (testPoint.Y - point.Y)*inverseNormalisation.Y).LengthSquared;
                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearestPoint = testPoint;
                }
            }

            return nearestPoint;
        }

        #endregion Public Methods

        #region Properties

        /// <summary>
        /// Gets if the current point is close enough to any of the other points
        /// </summary>
        public bool Locked
        {
            get { return locked; }
        }

        /// <summary>
        /// Gets the closest point
        /// </summary>
        public Point ClosestPoint
        {
            get { return closestPoint; }
        }

        /// <summary>
        /// Gets/Sets the minimum distance checked for
        /// </summary>
        public Size MinimumDistance
        {
            get { return minimumDistance.Size; }
            set { minimumDistance = new Rect(value); }
        }

        /// <summary>
        /// Gets/Sets the list of points checked against
        /// </summary>
        public List<Point> Points
        {
            get { return points; }
        }

        #endregion Properties

        #region Events and Event Triggers

        /// <summary>
        /// Event that gets fired when the closest point changes and is within
        /// minimum range
        /// </summary>
        public event EventHandler<ClosestPointArgs> ClosestPointChanged;

        /// <summary>
        /// Fires the ClosestPointChanged event
        /// </summary>
        protected void OnClosestPointChanged()
        {
            try
            {
                if (ClosestPointChanged != null)
                {
                    ClosestPointChanged(this, new ClosestPointArgs(locked, closestPoint));
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion Events and Event Triggers

        // ********************************************************************

        // ********************************************************************
        // Public Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************

        // ********************************************************************
        // Events and Event Triggers
        // ********************************************************************
    }
}