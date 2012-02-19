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

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Anj.XNA.Joysticks.Wizard
{
    /// <summary>
    /// Interaction logic for JoystickAxisValue.xaml
    /// </summary>
    public partial class JoystickAxisValue : UserControl
    {
        /// <summary>
        /// The <see cref="AxisName" /> dependency property's name.
        /// </summary>
        public const string AxisNamePropertyName = "AxisName";

        /// <summary>
        /// Gets or sets the value of the <see cref="AxisName" />
        /// property. This is a dependency property.
        /// </summary>
        public string AxisName
        {
            get { return (string)GetValue(AxisNameProperty); }
            set { SetValue(AxisNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AxisName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisNameProperty = DependencyProperty.Register(
            AxisNamePropertyName,
            typeof(string),
            typeof(JoystickAxisValue),
            new UIPropertyMetadata("Axis #"));



        public JoystickAxisValue()
        {
            InitializeComponent();
        }
    }
}