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
using System.Windows.Forms;
using Anj.Helpers.XNA;

#endregion

namespace Simulator.UI.WinForms
{
    public partial class PIDSettings : UserControl
    {
        ///// <summary>
        ///// Milliseconds that must pass between each time an event is allowed to fire to prevent spamming.
        ///// </summary>
        //private const int CoolDownMilliseconds = 100;

        // All sliders have the same internal range/resolution, but to the user this will appear to be adjustable
        // by the text boxes that present the virtual range.
        private const int SliderMin = 0;
        private const int SliderMax = 100;
        private const int Ticks = 10;
        private const int SliderTickFrequency = (SliderMax - SliderMin)/Ticks;

        private readonly object _syncRoot = new object();

        private readonly Binding _valueBindingD;
        private readonly Binding _valueBindingI;
        private readonly Binding _valueBindingP;

        #region Constructors

        public PIDSettings()
        {
            InitializeComponent();

            // Starting condition
            SliderMaxP.Text = SliderMaxI.Text = SliderMaxD.Text = "1";
            SliderMinP.Text = SliderMinI.Text = SliderMinD.Text = "-1";
            SliderP.Minimum = SliderI.Minimum = SliderD.Minimum = SliderMin;
            SliderP.Maximum = SliderI.Maximum = SliderD.Maximum = SliderMax;
            SliderP.TickFrequency = SliderI.TickFrequency = SliderD.TickFrequency = SliderTickFrequency;

            // Allow events to fire from the start.
            //_isUpdateReady = true;

            _valueBindingP = new Binding("Text", SliderP, "Value");
            _valueBindingI = new Binding("Text", SliderI, "Value");
            _valueBindingD = new Binding("Text", SliderD, "Value");

            // Always present slider value in the user-specified range
            // since the slider uses a fixed internal range of integers.
            _valueBindingP.Format += (sender, e) => InternalRangeToUserRange(e, SliderP, SliderMinP, SliderMaxP);
            _valueBindingI.Format += (sender, e) => InternalRangeToUserRange(e, SliderI, SliderMinI, SliderMaxI);
            _valueBindingD.Format += (sender, e) => InternalRangeToUserRange(e, SliderD, SliderMinD, SliderMaxD);

            // BindingComplete is used to trigger events that the PID settings have been changed.
            // This should prove more reliable than using TrackBar.ValueChanged event, because
            // this may trigger before the formatting of the textbox contents is complete.
            SliderP.ValueChanged += (sender, e) => FireUpdated();
            SliderI.ValueChanged += (sender, e) => FireUpdated();
            SliderD.ValueChanged += (sender, e) => FireUpdated();

            // TODO BindingComplete would be better, because it fires _after_ the conversion is done.
            // Slider.ValueChanged may probably fire before the value is formatted and entered in the textbox..
            // The problem is that BindingComplete never fires? I don't know why..

//            _valueBindingP.BindingComplete += _valueBindingP_BindingComplete;
//            _valueBindingI.BindingComplete += (sender, e) => FireUpdated();
//            _valueBindingD.BindingComplete += (sender, e) => FireUpdated();

            UserValueP.DataBindings.Add(_valueBindingP);
            UserValueI.DataBindings.Add(_valueBindingI);
            UserValueD.DataBindings.Add(_valueBindingD);

            SliderActiveP.CheckedChanged += (sender, e) => FireUpdated();
            SliderActiveI.CheckedChanged += (sender, e) => FireUpdated();
            SliderActiveD.CheckedChanged += (sender, e) => FireUpdated();
        }

        #endregion

        #region Public methods

        public void SetPID(float p, float i, float d)
        {
            SetPIDValue(p, SliderP, SliderMinP, SliderMaxP);
            SetPIDValue(i, SliderI, SliderMinI, SliderMaxI);
            SetPIDValue(d, SliderD, SliderMinD, SliderMaxD);

            // Force a update of the bindings because the slider value may not have changed
            _valueBindingP.ReadValue();
            _valueBindingI.ReadValue();
            _valueBindingD.ReadValue();
        }

        /// <summary>
        ///   Returns three float values in an array. P, I and D respectively.
        /// </summary>
        /// <returns></returns>
        public float[] GetPID()
        {
            var result = new float[3];

            result[0] = SliderActiveP.Checked ? GetPIDValue(SliderP, SliderMinP, SliderMaxP) : 0;
            result[1] = SliderActiveI.Checked ? GetPIDValue(SliderI, SliderMinI, SliderMaxI) : 0;
            result[2] = SliderActiveD.Checked ? GetPIDValue(SliderD, SliderMinD, SliderMaxD) : 0;
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        ///   Fire event only if enough time has passed since last event was fired, to avoid spamming events
        ///   when dragging sliders.
        /// </summary>
        private void FireUpdated()
        {
            lock (_syncRoot)
            {
                // If cooldown is still in progress, we don't fire any new events.
//                if (!_isUpdateReady)
//                {
                // Make sure any missed updates are accounted for after a cooldown.
//                    _isMissingUpdates = true;
//                    return;
//                }
//
                // Prevent new events from firing while we are asynchronously waiting for the cooldown.
//                _isUpdateReady = false;

                // Fire event
                if (PIDChanged != null)
                    PIDChanged();

                // Temporary timer to count down the cooldown time.
                // After cooldown, stop the temporary timer and allow new events to fire again.
//                var coolDownTimer = new Timer(CoolDownMilliseconds);
//                coolDownTimer.Elapsed += CoolDownTimer_Elapsed;
//                coolDownTimer.Start();
            }
        }

        //private void CoolDownTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    lock (_syncRoot)
        //    {
        //        var timer = (Timer) sender;
        //        timer.Stop();

        //        // Cooldown complete. Allow new updates again.
        //        _isUpdateReady = true;

        //        // Update automatically once more if updates were missing while waiting for cooldown.
        //        if (_isMissingUpdates)
        //        {
        //            _isMissingUpdates = false;
        //            BeginInvoke(new Action(FireUpdated));
        //        }
        //    }
        //}


        private static void InternalRangeToUserRange(ConvertEventArgs e, TrackBar slider, TextBox minTextBox,
                                                     TextBox maxTextBox)
        {
            e.Value = GetPIDValue(slider, minTextBox, maxTextBox);
        }

        private static void SetPIDValue(float value, TrackBar slider, TextBox minTextBox, TextBox maxTextBox)
        {
            float userMinValue;
            if (!float.TryParse(minTextBox.Text, out userMinValue)) return;

            float userMaxValue;
            if (!float.TryParse(maxTextBox.Text, out userMaxValue)) return;

            // Update limits for slider to span the new value. 
            // Deal with the special case when value == 0.
            float newMaxValue = (value == 0) ? 1 : value*2;
            float newMinValue = value - (newMaxValue - value);

            userMinValue = newMinValue;
            userMaxValue = newMaxValue;
            minTextBox.Text = newMinValue.ToString();
            maxTextBox.Text = newMaxValue.ToString();

            slider.Value =
                Convert.ToInt32(MyMathHelper.Lerp(value, userMinValue, userMaxValue, slider.Minimum, slider.Maximum));
        }

        private static float GetPIDValue(TrackBar slider, TextBox minTextBox, TextBox maxTextBox)
        {
            // Parse numeric values from GUI components.
            float userMinValue;
            if (!float.TryParse(minTextBox.Text, out userMinValue)) return float.NaN;

            float userMaxValue;
            if (!float.TryParse(maxTextBox.Text, out userMaxValue)) return float.NaN;

            // Map from internal slider range to the user-specified range.
            return MyMathHelper.Lerp(slider.Value, slider.Minimum, slider.Maximum, userMinValue, userMaxValue);
        }

        #endregion

        ///// <summary>
        /////   Is set to true if the PID settings were changed while the cooldown was not yet complete,
        /////   and causes an immediate update after the cooldown
        ///// </summary>
        //private bool _isMissingUpdates;

        /// <summary>
        ///   Fires whenever the PID settings is changed
        /// </summary>
        public event Action PIDChanged;
    }
}