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
    /// Interaction logic for JoystickAxes.xaml
    /// </summary>
    public partial class JoystickAxes : UserControl
    {
        /// <summary>
        /// The JoystickName attached property's name.
        /// </summary>
        public const string JoystickNamePropertyName = "JoystickName";

        /// <summary>
        /// Gets the value of the JoystickName attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the JoystickName property of the specified object.</returns>
        public static string GetJoystickName(DependencyObject obj)
        {
            return (string)obj.GetValue(JoystickNameProperty);
        }

        /// <summary>
        /// Sets the value of the JoystickName attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the JoystickName value of the specified object.</param>
        public static void SetJoystickName(DependencyObject obj, string value)
        {
            obj.SetValue(JoystickNameProperty, value);
        }

        /// <summary>
        /// Identifies the JoystickName attached property.
        /// </summary>
        public static readonly DependencyProperty JoystickNameProperty = DependencyProperty.RegisterAttached(
            JoystickNamePropertyName,
            typeof(string),
            typeof(JoystickAxes),
            new UIPropertyMetadata("My Joystick"));

        public JoystickAxes()
        {
            InitializeComponent();
        }
    }
}