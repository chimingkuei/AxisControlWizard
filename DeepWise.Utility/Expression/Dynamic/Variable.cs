using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DeepWise.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeepWise.Data;
using System.Windows;
using System.IO;

namespace DeepWise.Expression
{
    [JsonConverter(typeof(VariableJsonConverter))]
    public interface IVariable : INotifyPropertyChanged
    {
        string Name { get; }
        Type ValueType { get; }
        object Value { get; set; }
        object DefaultValue { get; set; }
    }

    public static class VariableExtensions
    {
        public static void ResetValue(this IVariable varible)
        {
            varible.Value = varible.DefaultValue;
        }
    }

    public class Variable<T> : IVariable , INotifyPropertyChanged
    {
        public Variable() { }
        public Variable(string name) :this() { Name = name; }
        public Variable(string name ,T defaultValue )
        {
            Name = name;
            _defaultValue = _value = defaultValue;
        }
        public string Name { get; set; }
        public Type ValueType => typeof(T);
        [JsonIgnore]
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }
        public T DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                NotifyPropertyChanged();
            }
        }
        object IVariable.DefaultValue { get => DefaultValue; set => DefaultValue = (T) value;}
        object IVariable.Value { get => Value; set => Value = (T)value; }

        T _value, _defaultValue;
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string name="")
        {
            PropertyChanged?.Invoke(name, new PropertyChangedEventArgs(name));
        }
    }

    public class VariableJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(IVariable));
        }
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var type = jObject["ValueType"].ToObject<Type>();
            var instanceType = typeof(Variable<>).MakeGenericType(type);
            return Activator.CreateInstance(instanceType, jObject["Name"].ToObject<string>(),jObject["DefaultValue"].ToObject(type));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class VariableTable : ObservableCollection<IVariable>, IEvaluationContext , IConfig
    {
        public static VariableTable Default { get; } = new VariableTable("varis");

        public VariableTable(string path, bool relativePath = true, bool encrypt = false)
        {
            if (relativePath)
                this.path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            else
                this.path = path;
            isEncrypted = encrypt;
            if (File.Exists(path))
            {
                try
                {
                    this.LoadConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"檔案讀取失敗:{path}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private string path;
        private bool isEncrypted;
        string IConfig.Path => path;
        bool IConfig.IsEncrypted => isEncrypted;

        public Dictionary<string, Type> GetIdentifiers()
        {
            var pairs = this.Select(x => new KeyValuePair<string, Type>(x.Name, x.ValueType));
            var dic = new Dictionary<string, Type>();
            foreach (var pair in pairs) dic.Add(pair.Key, pair.Value);
            return dic;
        }

        public object GetObject(string identifier) => this[identifier].Value;

        public IVariable this[string name] => this.First(x=>x.Name == name);
    }
}
