using DeepWise.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Test
{
    public class DemoObject : INotifyPropertyChanged
    {
        string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DemoObjectDerivative : DemoObject
    {
        public string ExtraProperty { get; set; }
    }

    public enum CustomIcon
    {
        [Icon(typeof(Properties.Resources), "baseline_note_add_black_24dp")]
        Add,
        [Icon(typeof(Properties.Resources), "baseline_file_copy_black_24dp")]
        Copy,
        [Icon(typeof(Properties.Resources), "baseline_drive_file_rename_outline_black_24dp")]
        Edit_Pen,
        [Icon(typeof(Properties.Resources), "baseline_delete_black_24dp")]
        Delete,

        [Icon("icons\\icon24_number0.png")]
        Number0,
        [Icon("icons\\icon24_number1.png"), Display(Name = "數字1")]
        Number1,
        [Icon("icons\\icon24_number2.png")]
        Number2,
        [Icon("icons\\icon24_number3.png")]
        Number3,
    }

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
