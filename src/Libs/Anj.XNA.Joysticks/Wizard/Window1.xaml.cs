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
using System.Windows.Interop;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

#endregion

namespace Anj.XNA.Joysticks.Wizard
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly List<Joystick> _joysticks;
        private readonly JoysticksVM _vm;
        private readonly JoystickWizardController _wizard;
        private readonly Timer _wizardUpdateTimer;

        public Window1()
        {
            InitializeComponent();

            _wizard = new JoystickWizardController();
//            _wizard.StepCompleted += WizardStepCompleted;
//            _wizard.ConfigurationComplete += WizardConfigurationComplete;
//
//            UpdateLabel(_wizard.CurrentAction);

            IntPtr windowHandle = new WindowInteropHelper(
                Application.Current.MainWindow).Handle;

            string[] joystickNames = Joystick.FindJoysticks();
            if (joystickNames == null || joystickNames.Length == 0)
            {
                MessageBox.Show("No joysticks connected! Plug in a joystick and restart the application.");
                return;
            }

            _joysticks = new List<Joystick>();
            foreach (var joystickName in joystickNames)
            {
                var js = new Joystick(windowHandle);
                js.AcquireJoystick(joystickName);
                _wizard.AddJoystick(js);
                _joysticks.Add(js);
            }

            _vm = new JoysticksVM();
            DataContext = _vm;

            for (int i = 0; i < _joysticks.Count; i++)
            {
                var joystickVM = new JoystickAxesVM { JoystickName = joystickNames[i] };
                _vm.Devices.Add(joystickVM);
            }


            _wizardUpdateTimer = new Timer(0.1);
            _wizardUpdateTimer.Elapsed += JoystickUpdateTimerElapsed;
            _wizardUpdateTimer.Start();

            Closing += Window1_Closing;
        }

        void WizardConfigurationComplete()
        {
            var map = _wizard.AxisToMapping;
            var axes = new List<JoystickAxis>(map.Keys);
            foreach (var axis in axes)
            {
                Console.WriteLine("Axis " + axis + ": " + map[axis].Positive + " and " + map[axis].Negative);
            }

            _wizardUpdateTimer.Stop();
        }

        void WizardStepCompleted(JoystickAction action, JoystickAction oppositeAction)
        {
            Console.WriteLine("Completed step: " + action + " and " + oppositeAction);

            UpdateLabel(_wizard.CurrentAction);
        }

        private void UpdateLabel(JoystickAction currentAction)
        {
            Dispatcher.Invoke(new Action(() => WizardInfo.Content = currentAction));
        }

        void JoystickUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_wizardUpdateTimer)
            {
                ((Timer)sender).Elapsed -= JoystickUpdateTimerElapsed;

                for (int i = 0; i < _joysticks.Count; i++)
                {
                    // First update the joystick
                    var joystick = _joysticks[i];
                    joystick.UpdateStatus();

                    // Then update the GUI to reflect the most recent joystick values
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "X").SetValue(joystick.State.X);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "Y").SetValue(joystick.State.Y);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "Z").SetValue(joystick.State.Z);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "Rx").SetValue(joystick.State.Rx);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "Ry").SetValue(joystick.State.Ry);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "Rz").SetValue(joystick.State.Rz);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "U").SetValue(joystick.State.GetSlider()[0]);
                    _vm.Devices[i].Axes.First(ax => ax.AxisName == "V").SetValue(joystick.State.GetSlider()[1]);
                }

                _wizard.Update();

                ((Timer)sender).Elapsed += JoystickUpdateTimerElapsed;
            }
        }

        void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var joystick in _joysticks)
            {
                joystick.ReleaseJoystick();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}