using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace Anj.XNA.Joysticks.Wizard
{
    public class JoysticksVM : ViewModelBase
    {
        /// <summary>
        /// The <see cref="Devices" /> property's name.
        /// </summary>
        public const string DevicesPropertyName = "Devices";

        private ObservableCollection<JoystickAxesVM> _myProperty = new ObservableCollection<JoystickAxesVM>();

        /// <summary>
        /// Gets the Devices property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<JoystickAxesVM> Devices
        {
            get { return _myProperty; }
            set
            {
                if (_myProperty == value)
                    return;

                _myProperty = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(DevicesPropertyName);
            }
        }
    }
}