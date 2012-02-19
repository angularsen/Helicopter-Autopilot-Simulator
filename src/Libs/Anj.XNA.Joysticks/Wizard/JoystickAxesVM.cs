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

using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

#endregion

namespace Anj.XNA.Joysticks.Wizard
{
    public class JoystickAxesVM : ViewModelBase
    {
        private readonly Joystick _joystickInterface;

        public JoystickAxesVM()
        {
            Axes = new ObservableCollection<AxisVM>();
            Axes.Add(new AxisVM() { AxisName = "X", PositiveHeight = 15});
            Axes.Add(new AxisVM() { AxisName = "Y", NegativeHeight = 15});
            Axes.Add(new AxisVM() { AxisName = "Z", NegativeHeight = 25});
            Axes.Add(new AxisVM() { AxisName = "Rx", PositiveHeight = 5, NegativeHeight = 5});
            Axes.Add(new AxisVM() { AxisName = "Ry", PositiveHeight = 5, NegativeHeight = 5});
            Axes.Add(new AxisVM() { AxisName = "Rz", PositiveHeight = 5, NegativeHeight = 5});
            Axes.Add(new AxisVM() { AxisName = "U", PositiveHeight = 0, NegativeHeight = 5});
            Axes.Add(new AxisVM() { AxisName = "V", PositiveHeight = 20, NegativeHeight = 0});
        }

        #region Dependency properties

        /// <summary>
        /// The <see cref="JoystickName" /> property's name.
        /// </summary>
        public const string JoystickNamePropertyName = "JoystickName";

        private string _joystickName = "Default name";

        /// <summary>
        /// Gets the JoystickName property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public string JoystickName
        {
            get
            {
                return _joystickName;
            }

            set
            {
                if (_joystickName == value)
                {
                    return;
                }

                _joystickName = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(JoystickNamePropertyName);
            }
        }


        public const string AxesPropertyName = "Axes";

        private ObservableCollection<AxisVM> _axes;

        public JoystickAxesVM(Joystick joystickInterface, string joystickName)
        {
            _joystickInterface = joystickInterface;
            JoystickName = joystickName;

            
        }

        /// <summary>
        /// Gets the Axes property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<AxisVM> Axes
        {
            get
            {
                return _axes;
            }

            set
            {
                if (_axes == value)
                {
                    return;
                }

                _axes = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(AxesPropertyName);
            }
        }

        #endregion

    }
}