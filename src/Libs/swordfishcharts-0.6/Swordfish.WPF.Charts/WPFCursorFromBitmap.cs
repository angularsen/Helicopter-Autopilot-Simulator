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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// This class converts a win32 bitmap to a WPF Cursor
    /// </summary>
    public class WPFCursorFromBitmap : SafeHandle
    {
        #region Methods

        /// <summary>
        /// Hidden contructor. Accessed only from the static method.
        /// </summary>
        /// <param name="cursorBitmap"></param>
        protected WPFCursorFromBitmap(Bitmap cursorBitmap)
            : base((IntPtr) (-1), true)
        {
            handle = cursorBitmap.GetHicon();
        }

        /// <summary>
        /// Creates a WPF cursor from a win32 bitmap
        /// </summary>
        /// <param name="cursorBitmap"></param>
        /// <returns></returns>
        public static Cursor CreateCursor(Bitmap cursorBitmap)
        {
            var csh = new WPFCursorFromBitmap(cursorBitmap);
            return CursorInteropHelper.Create(csh);
        }

        /// <summary>
        /// Releases the bitmap handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            bool result = DestroyIcon(handle);
            handle = (IntPtr) (-1);
            return result;
        }

        /// <summary>
        /// Imported from user32.dll. Destroys an icon GDI object.
        /// </summary>
        /// <param name="hIcon"></param>
        /// <returns></returns>
        [DllImport("user32")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets if the handle is valid or not
        /// </summary>
        public override bool IsInvalid
        {
            get { return handle == (IntPtr) (-1); }
        }

        #endregion Properties

        // ********************************************************************
        // Methods
        // ********************************************************************

        // ********************************************************************
        // Properties
        // ********************************************************************
    }
}