using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    public class AxisSpeedSetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DefaultValue(null)]
        public double? Speed
        {
            get => _speed;
            set
            {
                if (_speed == value) return;
                _speed = value;
                NotifyPropertyChanged();
            }
        }
        [DefaultValue(null)]
        public double? Acceleration
        {
            get => _acc;
            set
            {
                if (value == _acc) return;
                _acc = value;
                NotifyPropertyChanged();
            }
        }
        [DefaultValue(null)]
        public double? Deacceleration
        {
            get => _deacc;
            set
            {
                if (value == _deacc) return;
                _deacc = value;
                NotifyPropertyChanged();
            }
        }

        [DefaultValue(null)]
        public double? SpeedJog
        {
            get => _speedJog;
            set
            {
                if (_speedJog == value) return;
                _speedJog = value;
                NotifyPropertyChanged();
            }
        }
        [DefaultValue(null)]
        public double? AccelerationJog
        {
            get => _accJog;
            set
            {
                if (value == _accJog) return;
                _accJog = value;
                NotifyPropertyChanged();
            }
        }
        private double? _speed, _speedJog, _acc, _deacc, _accJog;
    }
}
