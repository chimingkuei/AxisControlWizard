using DeepWise.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Data
{
    public class DictionaryPointItem : INotifyPropertyChanged
    {
        public static ObservableCollection<DictionaryPointItem> GetList(IDictionary dic)
        {
            var c = new ObservableCollection<DictionaryPointItem>();
            foreach (var key in dic.Keys)
                c.Add(new DictionaryPointItem(dic, (string)key));

            c.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        foreach (DictionaryPointItem item in e.OldItems)
                        {
                            dic.Remove(item.Name);
                        }
                        break;
                    case NotifyCollectionChangedAction.Add:
                        break;
                    default:
                        throw new Exception();
                }
            };
            return c;
        }

        public DictionaryPointItem(IDictionary dic, string key)
        {
            this.dic = dic;
            Name = key;
        }
        public Type ItemType
        {
            get
            {
                var type = ((object)dic).GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                return type.GetGenericArguments()[1];
            }
        }
        public string Name { get; }

        public double X
        {
            get => dic[Name].X;
            set
            {
                var tmp = dic[Name];
                tmp.X = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
            }
        }

        public void Update()
        {
            switch (ItemType.Name)
            {
                case "Point":
                case "Vector":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
                    break;
                case "Point3D":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Z)));
                    break;
                case "Point6":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Z)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(C)));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public double Y
        {
            get => dic[Name].Y;
            set
            {
                var tmp = dic[Name];
                tmp.Y = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
            }
        }
        public double Z
        {
            get => dic[Name].Z;
            set
            {
                var tmp = dic[Name];
                tmp.Z = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Z)));
            }
        }
        public double A
        {
            get => dic[Name].A;
            set
            {
                var tmp = dic[Name];
                tmp.A = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
            }
        }
        public double B
        {
            get => dic[Name].B;
            set
            {
                var tmp = dic[Name];
                tmp.B = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B)));
            }
        }
        public double C
        {
            get => dic[Name].C;
            set
            {
                var tmp = dic[Name];
                tmp.C = value;
                dic[Name] = tmp;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(C)));
            }
        }

        public void SetValue(object value)
        {
            dic[Name] = value;
        }

        public object GetValue()
        {
            return dic[Name];
        }
        dynamic dic;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
