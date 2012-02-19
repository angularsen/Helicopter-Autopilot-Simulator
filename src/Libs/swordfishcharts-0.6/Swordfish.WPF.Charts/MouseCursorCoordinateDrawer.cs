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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// This class put coordinates on the mouse cursor
    /// </summary>
    public class MouseCursorCoordinateDrawer
    {
        #region Private Fields

        /// <summary>
        /// The bitmap being used for the cursor
        /// </summary>
        private Bitmap cursorBitmap;

        /// <summary>
        /// The cursor position. Used for calculating the relative position of the text.
        /// </summary>
        private Point cursorPosition;

        /// <summary>
        /// Flag to indicate that an exception was thrown when this tried to set
        /// the cursor, so do not try it again if false
        /// </summary>
        private bool hasPermissionToRun = true;

        /// <summary>
        /// The last coorinate painted on the cursor
        /// </summary>
        private Point lastCoordinate;

        /// <summary>
        /// The last cursor created. Needs to be disposed.
        /// </summary>
        private Cursor lastCursor;

        /// <summary>
        /// The closest point
        /// </summary>
        private Point lockPoint;

        /// <summary>
        /// Flag indicating if the coordinates are locked to the closest point or or not.
        /// </summary>
        private bool locked;

        /// <summary>
        /// Number of significant figures for the x coordinate
        /// </summary>
        private string xFormat = "";

        /// <summary>
        /// Number of significant figures for the y coordinate
        /// </summary>
        private string yFormat = "";

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        public MouseCursorCoordinateDrawer()
        {
            cursorBitmap = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// Set the coordinates on the cursor in an untransformed manner
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="uiElement"></param>
        public void SetCoordinatesOnCursor(Point mousePos, FrameworkElement uiElement)
        {
            SetCoordinatesOnCursor(mousePos, uiElement, new ScaleTransform(1, 1));
        }

        /// <summary>
        /// Sets the coordinates on the cursor using the transform passed in
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="uiElement"></param>
        /// <param name="transform"></param>
        public void SetCoordinatesOnCursor(Point mousePos, FrameworkElement uiElement, GeneralTransform transform)
        {
            if (transform != null && hasPermissionToRun)
            {
                try
                {
                    cursorPosition = mousePos;
                    lastCoordinate = transform.Transform(mousePos);
                    SetStringFormat(transform);
                    SetCoordinatesOnCursor(uiElement);
                }
                catch (Exception)
                {
                    hasPermissionToRun = false;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Works out the number of decimal places required to show the different between
        /// 2 pixels. E.g if pixels are .1 apart then use 2 places etc
        /// </summary>
        private void SetStringFormat(GeneralTransform transform)
        {
            var rect = new Rect(0, 0, 1, 1);
            rect = transform.TransformBounds(rect);

            var xFigures = (int) (Math.Ceiling(-Math.Log10(rect.Width)) + .1);
            var yFigures = (int) (Math.Ceiling(-Math.Log10(rect.Height)) + .1);
            xFormat = "#0.";
            yFormat = "#0.";
            for (int i = 0; i < xFigures; ++i)
            {
                xFormat += "#";
            }

            for (int i = 0; i < yFigures; ++i)
            {
                yFormat += "#";
            }
        }

        /// <summary>
        /// Changes the cursor for this control to show the coordinates
        /// </summary>
        /// <param name="uiElement"></param>
        private void SetCoordinatesOnCursor(FrameworkElement uiElement)
        {
            Point coordinate = locked ? lockPoint : lastCoordinate;
            Cursor newCursor = null;
            var cursorFont = new Font("Arial", 8f);

            try
            {
                // Lets get the string to be printed
                string coordinateText = coordinate.X.ToString(xFormat) + "," + coordinate.Y.ToString(yFormat);
                // Calculate the rectangle required to draw the string
                SizeF textSize = GetTextSize(coordinateText, cursorFont);

                // ok, so here's the minimum 1/4 size of the bitmap we need, as the
                // Hotspot for the cursor will be in the centre of the bitmap.
                int minWidth = 8 + (int) Math.Ceiling(textSize.Width);
                int minHeight = 8 + (int) Math.Ceiling(textSize.Height);

                // If the bitmap needs to be resized, then resize it, else just clear it
                if (cursorBitmap.Width < minWidth*2 || cursorBitmap.Height < minHeight*2)
                {
                    Bitmap oldBitmap = cursorBitmap;
                    cursorBitmap = new Bitmap(Math.Max(cursorBitmap.Width, minWidth*2),
                                              Math.Max(cursorBitmap.Height, minHeight*2));
                    oldBitmap.Dispose();
                }

                // Get the centre of the bitmap which will be the Hotspot
                var centre = new System.Drawing.Point(cursorBitmap.Width/2, cursorBitmap.Height/2);
                /// Calculate the text rectangle
                var textRectangle = new Rectangle(centre.X + 8, centre.Y + 8, minWidth - 8, minHeight - 8);

                int diff = (int) cursorPosition.X + textRectangle.Right/2 - 3 - (int) uiElement.ActualWidth;

                if (diff > 0)
                {
                    textRectangle.Location = new System.Drawing.Point(textRectangle.Left - diff, textRectangle.Top);
                }

                // Draw the target symbol, and the coordinate text on the bitmap
                using (Graphics g = Graphics.FromImage(cursorBitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    // This line causes a crash on laptops when you render a string
                    // g.CompositingMode = CompositingMode.SourceCopy;
                    g.Clear(Color.Transparent);

                    float targetX = centre.X;
                    float targetY = centre.Y;

                    float radius = 30;

                    if (!locked)
                    {
                        var blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 1.4f);
                        g.DrawEllipse(blackPen, targetX - radius*.5f, targetY - radius*.5f, radius, radius);
                        g.DrawLine(blackPen, targetX - radius*.8f, targetY, targetX - 2f, targetY);
                        g.DrawLine(blackPen, targetX + 2f, targetY, targetX + radius*.8f, targetY);
                        g.DrawLine(blackPen, targetX, targetY - radius*.8f, targetX, targetY - 2f);
                        g.DrawLine(blackPen, targetX, targetY + 2f, targetX, targetY + radius*.8f);
                    }
                    else
                    {
                        var blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 3f);
                        var yellowPen = new Pen(Color.FromArgb(255, 255, 255, 0), 2f);
                        g.DrawEllipse(blackPen, targetX - radius*.5f, targetY - radius*.5f, radius, radius);
                        g.DrawEllipse(yellowPen, targetX - radius*.5f, targetY - radius*.5f, radius, radius);
                    }

                    if (!locked)
                        g.FillRectangle(new SolidBrush(Color.FromArgb(127, 255, 255, 255)), textRectangle);
                    else
                        g.FillRectangle(new SolidBrush(Color.FromArgb(170, 255, 255, 0)), textRectangle);

                    // Setup the text format for drawing the subnotes
                    using (var stringFormat = new StringFormat())
                    {
                        stringFormat.Trimming = StringTrimming.None;
                        stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
                        stringFormat.Alignment = StringAlignment.Near;

                        // Draw the string left aligned
                        g.DrawString(
                            coordinateText,
                            cursorFont,
                            new SolidBrush(Color.Black),
                            textRectangle,
                            stringFormat);
                    }
                }

                // Now copy the bitmap to the cursor
                newCursor = WPFCursorFromBitmap.CreateCursor(cursorBitmap);
            }
            catch
            {
                Trace.WriteLine("Error drawing on cursor");
            }
            finally
            {
                // After the new cursor has been set, the unmanaged resources can be
                // cleaned up that were being used by the old cursor
                if (newCursor != null)
                {
                    uiElement.Cursor = newCursor;
                }
                if (lastCursor != null)
                {
                    lastCursor.Dispose();
                }
                lastCursor = newCursor;

                // Save the new values for cleaning up on the next pass
            }
        }

        /// <summary>
        /// Gets the rectangle taken up by the rendered text.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="font">The font to render the text in</param>
        /// <returns>The rectangle taken up by rendering the text</returns>
        private static RectangleF GetTextRectangle(string text, Font font)
        {
            RectangleF retRectangle;

            // Create a small bitmap just to get a device context
            var tmpBitmap = new Bitmap(1, 1);

            using (Graphics g = Graphics.FromImage(tmpBitmap))
            using (var stringFormat = new StringFormat())
            {
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.Trimming = StringTrimming.None;
                stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
                // Do a small rectangle. The size will be ignored with the flags being used
                var rectangle = new RectangleF(0, 0, 1, 1);
                // Set the stringFormat for measuring the first character
                CharacterRange[] characterRanges = {new CharacterRange(0, text.Length)};
                stringFormat.SetMeasurableCharacterRanges(characterRanges);

                Region[] stringRegions = g.MeasureCharacterRanges(text, font, rectangle, stringFormat);

                // Draw rectangle for first measured range.
                retRectangle = stringRegions[0].GetBounds(g);
            }
            return retRectangle;
        }

        /// <summary>
        /// Gets the width and height of the rendered text.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="font">The font to render the text in</param>
        /// <returns>The size of the rendered text</returns>
        private static SizeF GetTextSize(string text, Font font)
        {
            RectangleF rectangle = GetTextRectangle(text, font);
            var size = new SizeF(rectangle.Right, rectangle.Bottom);
            return size;
        }

        #endregion Private Methods

        #region Properties

        /// <summary>
        /// Gets/Sets if the coordinates are locked to the LockPoint or not
        /// </summary>
        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }

        /// <summary>
        /// Gets/Sets the coordinate for the cursor to show when it is "Locked"
        /// </summary>
        public Point LockPoint
        {
            get { return lockPoint; }
            set { lockPoint = value; }
        }

        #endregion Properties

        // ********************************************************************
        // Private Fields
        // ********************************************************************

        // ********************************************************************
        // Public Methods
        // ********************************************************************

        // ********************************************************************
        // Private Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************
    }
}