using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Data;

namespace MotionControllers.Motion
{
    [DebuggerDisplay("({SlaveID},{Channel}) ({Type})")]
    public struct IOPortInfo : IEquatable<IOPortInfo>
    {
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            if ((Type & IOTypes.General) != IOTypes.General)
                SlaveID = -1;
        }
        public IOPortInfo(int slaveID, int channel, IOTypes type)
        {
            if (type.HasFlag(IOTypes.General)) throw new ArgumentException();
            if((type & (IOTypes.Input | IOTypes.Output)) == (IOTypes.Input | IOTypes.Output)) throw new ArgumentException();
            if ((type & (IOTypes.Digital | IOTypes.Analog)) == (IOTypes.Digital | IOTypes.Analog)) throw new ArgumentException();
            SlaveID = slaveID;
            Channel = channel;
            Type = type | IOTypes.EtherCAT;
        }

        public IOPortInfo(int channel, IOTypes type)
        {
            if (type.HasFlag(IOTypes.EtherCAT)) throw new ArgumentException();
            if ((type & (IOTypes.Input | IOTypes.Output)) == (IOTypes.Input | IOTypes.Output)) throw new ArgumentException();
            if ((type & (IOTypes.Digital | IOTypes.Analog)) == (IOTypes.Digital | IOTypes.Analog)) throw new ArgumentException();
            SlaveID = -1;
            Channel = channel;
            Type = type | IOTypes.General;
        }
        public override bool Equals(object obj) => obj is IOPortInfo info && info == this;

        public static bool operator ==(IOPortInfo a, IOPortInfo b)
        {
            return a.Type == b.Type && a.Channel == b.Channel && (a.Type.HasFlag(IOTypes.General) || a.SlaveID == b.SlaveID);
        }
        public static bool operator !=(IOPortInfo a, IOPortInfo b) => !(a == b);

        public IOTypes Type { get; set; }
        public int Channel { get; set; }

        public bool ShouldSerializeSlaveID() => Type.HasFlag(IOTypes.EtherCAT);
        public int SlaveID { get; set; }

        public override string ToString()
        {
            string s = Type.HasFlag(IOTypes.EtherCAT) ? "E" : "";
            s += Type.HasFlag(IOTypes.Digital) ? "D" : "A";
            s += Type.HasFlag(IOTypes.Input) ? "I" : "O";
            s += $"-{Channel}";
            if(Type.HasFlag(IOTypes.EtherCAT)) s+=$"(slave{SlaveID})"; 
            return s;
        }

        public string ToString(ADLINK_Motion cntlr)
        {
            var _this = this;
            return cntlr.IOTable.First(x=>x.Value== _this).Key;
        }


        public bool Equals(IOPortInfo other)
        {
            return Type == other.Type &&
                   Channel == other.Channel &&
                   SlaveID == other.SlaveID;
        }

        public override int GetHashCode()
        {
            int hashCode = -778764546;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Channel.GetHashCode();
            hashCode = hashCode * -1521134295 + SlaveID.GetHashCode();
            return hashCode;
        }
    }

    [DebuggerDisplay("{Name} : {Value}")]
    public class IOPort<T> : INotifyPropertyChanged , IDisposable
    {
        public ADLINK_Motion Controller { get; }
        public IOPortInfo Info { get; }
        public IOPort(ADLINK_Motion cntlr, IOPortInfo info)
        {
            if(!(typeof(T) == typeof(bool) || typeof(T) == typeof(double)))
                    throw new ArgumentException("IOPort<T> where T must be Double or Boolean");
            Controller = cntlr ?? throw new ArgumentNullException();
            Info = info;
            cntlr.IOStatusChanged += Cntlr_IOStatusChanged;
        }

        private void Cntlr_IOStatusChanged(object sender, IOEventArgs e)
        {
            if (e.Match(Info))
            {
                NotifyPropertyChanged("Value");
            }
        }

        public string Name
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                var pair = Controller.IOTable.FirstOrDefault(x => x.Value == Info);
                if (pair.Value != null)
                    return pair.Key;
                else
                    return null;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                var dic = Controller.IOTable;
                if (dic.ContainsKey(value)) throw new Exception("字典中已含有key:" + value);
                var pair = dic.FirstOrDefault(x => x.Value == Info);
                if (pair.Value != null)
                {
                    dic.Remove(pair.Key);
                    dic.Add(value, pair.Value);
                }
            }
        }
        public int SlaveID
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                return Info.SlaveID;
            }
        }
        public int Channel
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                return Info.Channel;
            }
        }
        public IOTypes Flags
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                return Info.Type;
            }
        }

        public T Value
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                ArgumentException tmp = null;
                try
                {
                    return Controller.GetIOValue<T>(Info);
                }
                catch(ArgumentException ex)
                {
                    if (ex == tmp)
                        throw ex;
                    else
                        return default;
                }
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                if (Flags.HasFlag(IOTypes.Input)) throw new Exception("a \"input\" signal value can not be set");

                ArgumentException tmp = null;
                try
                {
                    Controller.SetOutputValue(Info, value);
                }
                catch (ArgumentException ex)
                {
                    if (ex == tmp)
                        throw ex;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool IsDisposed { get; private set; } = false;
        public void Dispose()
        {
            if(IsDisposed)throw new ObjectDisposedException(GetType().FullName);
                Controller.IOStatusChanged -= Cntlr_IOStatusChanged;
            IsDisposed = true;
        }
    }

    public class IODictionary : Dictionary<string,IOPortInfo>
    {
        public IODictionary()
        {

        }
        [JsonIgnore]
        public string Path { get; }
        public IODictionary(string path)
        {
            Path = path;
        }

        public bool HasChanged()
        {
            this.Sort();

            if (System.IO.File.Exists(Path))
                return JsonConvert.SerializeObject(this) != System.IO.File.ReadAllText(Path);
            else
                return this.Any(x => x.Key != x.Value.ToString());
        }

        public void Load()
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<string, IOPortInfo>>(System.IO.File.ReadAllText(Path)).OrderBy(x => GetRank(x.Value));

            var crt = this.Values.Cast<IOPortInfo>().OrderBy(GetRank);
            var load = dic.Select(x=>x.Value).OrderBy(GetRank);
            var b = crt.First() == load.First();
            if (load.SequenceEqual(crt))
            {
                Clear();
                foreach (var item in dic)
                    Add(item.Key,item.Value);
            }
            else
            {
                throw new Exception("存檔的IO Map不符合");
            }
            //Sort();
        }

        public void Save()
        {
            System.IO.File.WriteAllText(Path, JsonConvert.SerializeObject(this));
        }

        [DebuggerStepThrough]
        public void Sort()
        {
            var sorted = this.ToList().OrderBy(x => GetRank(x.Value));
            this.Clear();
            foreach (var item in sorted)
                this.Add(item.Key, item.Value);
        }

        static int GetRank(IOPortInfo info)
        {
            int acc = 0;
            acc += (int)info.Type << 16;
            if (info.Type.HasFlag(IOTypes.EtherCAT))
                acc += (int)info.SlaveID * 8;
            acc += (int)info.Channel * 1;
            return acc;
        }
    }

    [Flags,JsonConverter(typeof(StringEnumConverter))]
    public enum IOTypes : byte
    {
        General =
            0b0001_0000,
        EtherCAT =
            0b0010_0000,

        Digital =
            0b0000_0100,
        Analog =
            0b0000_1000,

        Input =
            0b0000_0001,
        Output =
            0b0000_0010,
    }

    public class IOEventArgs : EventArgs
    {
        public IOEventArgs(IOTypes type,int channel, object value)
        {
            switch (type)
            {
                case IOTypes.General | IOTypes.Digital | IOTypes.Input:
                case IOTypes.General | IOTypes.Digital | IOTypes.Output:
                case IOTypes.General | IOTypes.Analog | IOTypes.Input:
                case IOTypes.General | IOTypes.Analog | IOTypes.Output:
                    break;
                default:
                    throw new ArgumentException();
            }
            Type = type;
            Channel = channel;
            Value = value;
        }
        public IOEventArgs(IOTypes type,int slaveId, int channel, object value)
        {
            switch (type)
            {
                case IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input:
                case IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output:
                case IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input:
                case IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output:
                    break;
                default:
                    throw new ArgumentException();
            }
            Type = type;
            _slaveID = slaveId;
            Channel = channel;
            Value = value;
        }
        public bool IsDigital => Type.HasFlag(IOTypes.Digital);
        public bool IsAnalog => Type.HasFlag(IOTypes.Analog);
        public bool IsGeneral => Type.HasFlag(IOTypes.General);
        public bool IsEtherCAT => Type.HasFlag(IOTypes.EtherCAT);
        public bool IsInputSignal => Type.HasFlag(IOTypes.Input);
        public bool IsOutputSignal => Type.HasFlag(IOTypes.Output);

        public IOTypes Type { get; }
        public int SlaveID => Type.HasFlag(IOTypes.EtherCAT) ? _slaveID : throw new Exception("非EtherCAT類型的I/O，無法獲得SlaveID之值");
        public int Channel { get; }
        public object Value { get; }
        public bool Match(IOPortInfo info) => info.Type == Type && info.Channel == Channel && (!Type.HasFlag(IOTypes.EtherCAT) || info.SlaveID == SlaveID);
        int _slaveID = -1;
    }

    public class IONotFoundException : Exception
    {

        public IONotFoundException(string name, IOTypes types = 0) : base(GetDescription(name, types))
        {

        }

        static string GetDescription(string name, IOTypes types = 0)
        {
            switch (types)
            {
                case IOTypes.Output:
                    return $"找不到名稱為'{name}'的Output點位";
                case IOTypes.Input:
                    return $"找不到名稱為'{name}'的Input點位";
                default:
                    return $"找不到名稱為'{name}'的IO點位({types})";
                case 0:
                    return $"找不到名稱為'{name}'的IO點位";
            }
        }
    }

}
