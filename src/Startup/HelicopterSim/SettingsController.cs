#if !XBOX
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
using System.Collections.Generic;
using System.Windows.Forms;
using Control;
using Simulator.Resources;
using Simulator.Scenarios;
using Simulator.UI.WinForms;

#endregion

namespace HelicopterSim
{
    public class SettingsController
    {
        private readonly Form _gameForm;
        private readonly ComboBox _pidListUI;
        private readonly PIDSettings _pidSettings;
        private readonly IList<PIDSetup> _pidSetups;
        private readonly ComboBox _scenarioListUI;
        private readonly IList<Scenario> _scenarios;
        private readonly SettingsForm _settingsForm;
        private PID _currentPID;

        public SettingsController(Form gameForm)
        {
            if (gameForm == null) throw new ArgumentNullException("gameForm");

            _gameForm = gameForm;

            // Create a settings form to enable the user to change settings in-game
            _settingsForm = new SettingsForm();
            _pidSetups = SimulatorResources.GetPIDSetups();
            _scenarios = SimulatorResources.GetScenarios();

            if (_pidSetups == null || _pidSetups.Count == 0)
                throw new Exception("No PID setups were found!");

            CurrentPIDSetup = _pidSetups[0];

            _pidSettings = _settingsForm.PIDSettings;
            _pidListUI = _pidSettings.PIDSetup;
            _scenarioListUI = _settingsForm.SimSettings.Scenarios;

            _scenarioListUI.SelectedValueChanged += Scenarios_SelectedValueChanged;
            _pidListUI.SelectedValueChanged += SelectedPIDChanged;

            _pidSettings.PIDChanged += PIDValuesChanged;

            Populate();
        }

        /// <summary>
        /// Returns the currently selected PID setup.
        /// </summary>
        public PIDSetup CurrentPIDSetup { get; private set; }

        public event Action PIDSettingsChanged;

        private void Populate()
        {
            foreach (Scenario scenario in _scenarios)
                _settingsForm.SimSettings.Scenarios.Items.Add(scenario);

            foreach (PID pid in _pidSetups[0])
                _pidListUI.Items.Add(pid);

            _scenarioListUI.SelectedIndex = 0;
            _pidListUI.SelectedIndex = 0;
        }

        private static void Scenarios_SelectedValueChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;

            var scenario = cb.SelectedItem as Scenario;
            if (scenario != null)
            {
            }
        }

        /// <summary>
        /// Make sure to update the current PID object to reflect the changes in GUI.
        /// Also propagate the event to the outside of this class.
        /// </summary>
        private void PIDValuesChanged()
        {
            // Update model copy of currently selected PID
            float[] pidValues = _pidSettings.GetPID();
            _currentPID.P = pidValues[0];
            _currentPID.I = pidValues[1];
            _currentPID.D = pidValues[2];

            if (PIDSettingsChanged != null) PIDSettingsChanged();
        }

        /// <summary>
        /// The user selected a PID from the list, and this method presents the values for the PID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedPIDChanged(object sender, EventArgs e)
        {
            _currentPID = (PID) _pidSettings.PIDSetup.SelectedItem;
            _pidSettings.SetPID(_currentPID.P, _currentPID.I, _currentPID.D);
        }

        public void ShowSettingsDialog()
        {
            if (_settingsForm != null)
                _settingsForm.Show(_gameForm);
        }

        public void HideSettingsDialog()
        {
            if (_settingsForm != null)
                _settingsForm.Hide();
        }
    }
}
#endif
