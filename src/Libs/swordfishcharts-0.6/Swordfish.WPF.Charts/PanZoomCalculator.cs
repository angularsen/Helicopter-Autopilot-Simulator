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
using System.Windows;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// Event Arguments containing the updated pan and zoom
    /// </summary>
    internal class PanZoomArgs : EventArgs
    {
        /// <summary>
        /// The current pan
        /// </summary>
        private readonly Point currentPan;

        /// <summary>
        /// The current zoom
        /// </summary>
        private readonly Point currentZoom;

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        /// <param name="currentPan"></param>
        /// <param name="currentZoom"></param>
        internal PanZoomArgs(Point currentPan, Point currentZoom)
        {
            this.currentPan = currentPan;
            this.currentZoom = currentZoom;
        }

        public Point Pan
        {
            get { return currentPan; }
        }

        public Point Zoom
        {
            get { return currentZoom; }
        }
    }

    /// <summary>
    /// This class handles calculating the pan and zoom for a control. Implements
    /// it like a state machine.
    /// </summary>
    internal class PanZoomCalculator
    {
        #region Private Fields

        /// <summary>
        /// The current pan position
        /// </summary>
        private Point currentPan;

        /// <summary>
        /// The current zoom position
        /// </summary>
        private Point currentZoom;

        /// <summary>
        /// Flag that when set flips the Y axis such that zero is in the
        /// top left and positive is down.
        /// </summary>
        private bool flipYAxis = false;

        /// <summary>
        /// Flag indicating if the left mouse button is down
        /// </summary>
        private bool isPanning = false;

        /// <summary>
        /// Flag indicating if the right mouse button is down
        /// </summary>
        private bool isZooming = false;

        /// <summary>
        /// The starting point that the mouse was at when the user started
        /// zooming or panning
        /// </summary>
        private Point lastPosition;

        /// <summary>
        /// The windows dimensions that we are zooming/panning in
        /// </summary>
        private Rect window;

        /// <summary>
        /// The cursor position that the user is zooming in on
        /// </summary>
        private Point zoomCentre;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        internal PanZoomCalculator(Rect window)
        {
            // Initialize class fields
            currentPan = new Point(0, 0);
            currentZoom = new Point(1, 1);

            this.window = window;
        }

        /// <summary>
        /// Call this to start panning
        /// </summary>
        /// <param name="position"></param>
        public void StartPan(Point position)
        {
            lastPosition = position;
            isPanning = true;
        }

        /// <summary>
        /// Call this to start zooming
        /// </summary>
        /// <param name="position"></param>
        public void StartZoom(Point position)
        {
            zoomCentre = position;
            lastPosition = position;
            isZooming = true;

            MouseMoved(position);
        }

        /// <summary>
        /// Call this to Stop Panning
        /// </summary>
        public void StopPanning()
        {
            isPanning = false;
            OnPanZoomChanged();
        }

        /// <summary>
        /// Call this to Stop Zooming
        /// </summary>
        public void StopZooming()
        {
            isZooming = false;
            OnPanZoomChanged();
        }

        /// <summary>
        /// Event handler for when the mouse moves over the control.
        /// Changes the tool tip to show the graph coordinates at the
        /// current mouse point, and does zooming and panning.
        /// </summary>
        public void MouseMoved(Point newPosition)
        {
            if (isPanning)
            {
                currentPan.X += (newPosition.X - lastPosition.X)/currentZoom.X/window.Width;
                currentPan.Y += flipYAxis
                                    ?
                                        (lastPosition.Y - newPosition.Y)/currentZoom.Y/window.Height
                                    :
                                        (newPosition.Y - lastPosition.Y)/currentZoom.Y/window.Height;
            }

            if (isZooming)
            {
                Point oldZoom = currentZoom;

                currentZoom.X *= Math.Pow(1.002, newPosition.X - lastPosition.X);
                currentZoom.Y *= Math.Pow(1.002, -newPosition.Y + lastPosition.Y);

                // this limits the zoom
                currentZoom.X = Math.Max(1, currentZoom.X);
                currentZoom.Y = Math.Max(1, currentZoom.Y);

                currentPan.X += (window.Width*.5 - zoomCentre.X)*(1/oldZoom.X - 1/currentZoom.X)/window.Width;
                currentPan.Y += flipYAxis
                                    ?
                                        (window.Height*.5 - zoomCentre.Y)*(1/oldZoom.Y - 1/currentZoom.Y)/window.Height
                                    :
                                        (-window.Height*.5 - zoomCentre.Y)*(1/oldZoom.Y - 1/currentZoom.Y)/window.Height;
            }

            lastPosition = newPosition;

            if (isPanning || isZooming)
            {
                // Limit Pan
                var maxPan = new Point();
                maxPan.X = .5*(currentZoom.X - 1)/(currentZoom.X);
                maxPan.Y = .5*(currentZoom.Y - 1)/(currentZoom.Y);
                currentPan.X = Math.Min(maxPan.X, currentPan.X);
                currentPan.X = Math.Max(-maxPan.X, currentPan.X);
                currentPan.Y = Math.Min(maxPan.Y, currentPan.Y);
                currentPan.Y = Math.Max(-maxPan.Y, currentPan.Y);

                if (Double.IsNaN(currentPan.X) || Double.IsNaN(currentPan.Y))
                    currentPan = new Point(0f, 0f);
                if (Double.IsNaN(currentZoom.X) || Double.IsNaN(currentZoom.Y))
                    currentZoom = new Point(1f, 1f);

                OnPanZoomChanged();
            }
        }

        /// <summary>
        /// Call this to reset the Pan/Zoom state machine
        /// </summary>
        public void Reset()
        {
            // Reset zoom and pan
            isPanning = false;
            isZooming = false;
            currentZoom = new Point(1, 1);
            currentPan = new Point(0, 0);
            OnPanZoomChanged();
        }

        #endregion Public Methods

        #region Properties

        /// <summary>
        /// Gets whether panning is activated or not
        /// </summary>
        public bool IsPanning
        {
            get { return isPanning; }
        }

        /// <summary>
        /// Gets whether zooming is activated or not
        /// </summary>
        public bool IsZooming
        {
            get { return isZooming; }
        }

        /// <summary>
        /// Gets/Sets the window rectangle that is being zoomed in
        /// </summary>
        public Rect Window
        {
            get { return window; }
            set { window = value; }
        }

        /// <summary>
        /// Gets the current pan
        /// </summary>
        public Point Pan
        {
            get { return currentPan; }
            set
            {
                if (currentPan != value)
                {
                    currentPan = value;
                    OnPanZoomChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current zoom
        /// </summary>
        public Point Zoom
        {
            get { return currentZoom; }
            set
            {
                if (currentZoom != value)
                {
                    currentZoom = value;
                    OnPanZoomChanged();
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether to flip the Y axis or not
        /// </summary>
        public bool FlipYAxis
        {
            get { return flipYAxis; }
            set { flipYAxis = value; }
        }

        #endregion Properties

        #region Events and Triggers

        /// <summary>
        /// Gets fired when the pan or zoom changes
        /// </summary>
        public event EventHandler<PanZoomArgs> PanZoomChanged;

        /// <summary>
        /// Fires the PanZoomChanged event
        /// </summary>
        protected void OnPanZoomChanged()
        {
            try
            {
                if (PanZoomChanged != null)
                {
                    PanZoomChanged(this, new PanZoomArgs(currentPan, currentZoom));
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion Events and Triggers

        // ********************************************************************
        // Private Fields
        // ********************************************************************

        // ********************************************************************
        // Public Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************

        // ********************************************************************
        // Events and Triggers
        // ********************************************************************
    }
}