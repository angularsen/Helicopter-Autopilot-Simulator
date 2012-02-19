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
using GalaSoft.MvvmLight;

#endregion

namespace Anj.XNA.Joysticks.Wizard
{
    public class AxisVM : ViewModelBase
    {
        private int _positiveHeight;

        /// <summary>
        /// Gets the PositiveHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PositiveHeight
        {
            get { return _positiveHeight; }
            set
            {
                if (_positiveHeight == value)
                    return;

                _positiveHeight = value;

                // Update bindings, no broadcast
                RaisePropertyChanged("PositiveHeight");
            }
        }

        private int _negativeHeight;

        /// <summary>
        /// Gets the NegativeHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int NegativeHeight
        {
            get { return _negativeHeight; }
            set
            {
                if (_negativeHeight == value)
                    return;

                _negativeHeight = value;

                // Update bindings, no broadcast
                RaisePropertyChanged("NegativeHeight");
            }
        }

        /// <summary>
        /// The <see cref="AxisName" /> property's name.
        /// </summary>
        public const string AxisNamePropertyName = "AxisName";

        private string _axisName = "";

        /// <summary>
        /// Gets the AxisName property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public string AxisName
        {
            get { return _axisName; }
            set
            {
                if (_axisName == value)
                    return;

                _axisName = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(AxisNamePropertyName);
            }
        }

        public void SetValue(int value)
        {
            float floatValue = (float)(value - Int16.MaxValue) / Int16.MaxValue; // -1 to +1
            const int maxRectHeight = 22;
            int height = Convert.ToInt32(maxRectHeight * floatValue);
            PositiveHeight = Math.Max(0, height);
            NegativeHeight = Math.Max(0, -height);
        }
    }
}