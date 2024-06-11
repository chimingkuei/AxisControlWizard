using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static ADLINK_DEVICE.APS168;
namespace MotionControllers.Motion
{
    public class KeyedLocations : ObservableCollection<NamedLocation>
    {
        public int this[string name]
        {
           get
            {
                
                try
                {
                    return this.First(x => x.Name == name);
                }
                catch(InvalidOperationException ex)
                {
                    throw new Exception($"不存在名稱為'{name}'的點位", ex);
                }
            }
            set
            {
                try
                {
                    this.First(x => x.Name == name).Location = value;
                }
                catch (InvalidOperationException ex)
                {
                    throw new Exception($"不存在名稱為'{name}'的點位", ex);
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                //foreach(var added in e.NewItems.Cast<NamedLocation>().Select(x => x.Name))
                //{
                //    if (this.Select(x => x.Name).Contains(added))
                //        throw new Exception($"集合中已含有名稱為'{added}'的點位");
                //}
            }
        }
    }

    [DebuggerDisplay("{Name} : {Location}")]
    public class NamedLocation : INotifyPropertyChanged
    {
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public int Location
        {
            get => _location;
            set
            {
                if (_location == value) return;
                _location = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Location"));
            }
        }
        public static implicit operator int(NamedLocation namedLocation)=> namedLocation.Location;
        string _name;
        int _location;
        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => $"{Name} : {Location}";
    }
}