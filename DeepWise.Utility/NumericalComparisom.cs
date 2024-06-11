using DeepWise.Localization;
using DeepWise.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise
{
    /// <summary>
    /// 表示判斷數值是否落在範圍的標準，使用方法<see cref="Predicate(T)"/>來計算結果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NumericalComparison<T> : IEquatable<NumericalComparison<T>>, ISerializable where T : IComparable<T>
    {
        public NumericalComparison(RelationOperator comparison, T value)
        {
            _comparison = comparison;
            _value1 = value;
            _value2 = default(T);
        }
        public NumericalComparison(RelationOperator comparison, T value1, T value2)
        {
            _comparison = comparison;
            _value1 = value1;
            _value2 = value2;
        }
        public NumericalComparison(SerializationInfo info, StreamingContext context)
        {
            _comparison = (RelationOperator)info.GetValue(nameof(Comparison), typeof(RelationOperator));
            _value1 = (T)info.GetValue(nameof(Value1), typeof(T));
            _value2 = (T)info.GetValue(nameof(Value2), typeof(T));
        }

        public bool Ranged => _comparison == RelationOperator.InRange | _comparison == RelationOperator.OutRange;
        [JsonIgnore]
        public RelationOperator Comparison
        {
            get => _comparison;
            set
            {
                if (_comparison == value) return;
                bool oriState = Ranged;
                _comparison = value;
                bool newState = Ranged;
                if (oriState != newState)
                {
                    if (newState)
                        _value2 = _value1;
                    else
                        _value2 = default(T);
                }
            }
        }
        [JsonIgnore]
        public T Value1
        {
            get => _value1;
            set
            {
                if (Object.Equals(_value1, value)) return;
                _value1 = value;
            }
        }
        [JsonIgnore]
        public T Value2
        {
            get => _value2;
            set
            {
                if (!Ranged)
                {
                    throw new Exception(nameof(NumericalComparison<T>) + ": 當前的比較子不支援第二個數值！");
                }
                if (Object.Equals(_value2, value)) return;
                _value2 = value;
            }
        }
        public static bool operator ==(NumericalComparison<T> a, NumericalComparison<T> b) => a.Equals(b);
        public static bool operator !=(NumericalComparison<T> a, NumericalComparison<T> b) => !a.Equals(b);
        public override bool Equals(object obj) => (obj is NumericalComparison<T> criterion && criterion == this);
        public bool Equals(NumericalComparison<T> other) => Comparison == other.Comparison && Object.Equals(Value1, other.Value1) && Object.Equals(Value2, other.Value2);
     
        public bool Predicate(T value)
        {
            switch (Comparison)
            {
                case RelationOperator.Equal: return value.CompareTo(Value1) == 0;
                case RelationOperator.NotEqual: return value.CompareTo(Value1) != 0;
                case RelationOperator.Greater: return value.CompareTo(Value1) > 0;
                case RelationOperator.GreaterOrEqual: return value.CompareTo(Value1) >= 0;
                case RelationOperator.Less: return value.CompareTo(Value1) < 0;
                case RelationOperator.LessOrEqual: return value.CompareTo(Value1) <= 0;
                case RelationOperator.InRange: return value.CompareTo(Value1) * value.CompareTo(Value2) <= 0;
                case RelationOperator.OutRange: return value.CompareTo(Value1) * value.CompareTo(Value2) > 0;
                default: throw new Exception();
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Comparison), Comparison);
            info.AddValue(nameof(Value1), Value1);
            info.AddValue(nameof(Value2), Value2);
        }

        public override int GetHashCode()
        {
            var hashCode = -1350531696;
            hashCode = hashCode * -1521134295 + _comparison.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_value1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_value2);
            return hashCode;
        }
        public override string ToString()
        {
            switch (Comparison)
            {
                case RelationOperator.Equal:
                case RelationOperator.Greater:
                case RelationOperator.GreaterOrEqual:
                case RelationOperator.Less:
                case RelationOperator.LessOrEqual:
                    return Comparison.GetDisplayName() + Value1;
                case
                    RelationOperator.InRange:
                    return Comparison.GetDisplayName() + Value1 + "~" + Value2;
                default:
                    throw new NotImplementedException();
            }
        }
        public static NumericalComparison<T> Parse(string str)
        {
            var dic = new Dictionary<string, RelationOperator>();
            foreach (RelationOperator value in Enum.GetValues(typeof(RelationOperator))) dic.Add(value.GetDisplayName(), value);
            var matchC = new List<string>();
            foreach (string comparison in dic.Keys)
                if (str.Contains(comparison)) matchC.Add(comparison);

            if (matchC.Count >= 1)
            {
                var s = matchC.Count == 1 ? matchC[0] : matchC.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
                var eValue = dic[s];
                switch (eValue)
                {
                    case RelationOperator.Equal:
                    case RelationOperator.Greater:
                    case RelationOperator.GreaterOrEqual:
                    case RelationOperator.Less:
                    case RelationOperator.LessOrEqual:
                        {
                            var number = str.Replace(s, "");
                            return new NumericalComparison<T>(eValue, ParseNumber(number));
                        }
                    case RelationOperator.InRange:
                        {
                            var match = System.Text.RegularExpressions.Regex.Matches(str, @"[-+]?[0-9]*\.?[0-9]+");
                            if (match.Count == 2)
                                return new NumericalComparison<T>(eValue, ParseNumber(match[1].Value), (T)(object)Int32.Parse(match[1].Value));
                            else
                                throw new Exception();
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                throw new Exception("無法解析字串\"" + str + "\"");
        }
        static T ParseNumber(string number)
        {
            if (typeof(T) == typeof(Int32))
                return (T)(object)Int32.Parse(number);
            else if (typeof(T) == typeof(Single))
                return (T)(object)Single.Parse(number);
            else if (typeof(T) == typeof(Double))
                return (T)(object)Double.Parse(number);
            else
                throw new Exception();
        }
        [JsonProperty(nameof(Comparison))]
        RelationOperator _comparison;
        [JsonProperty(nameof(Value1))]
        T _value1;
        [JsonProperty(nameof(Value2))]
        T _value2;
    }

    [LocalizedDisplayName(nameof(RelationOperator), typeof(Resources))]
    public enum RelationOperator
    {
        [LocalizedDisplayName("RelationalOperator.Equal", typeof(Resources))]
        Equal,
        [LocalizedDisplayName("RelationalOperator.NotEqual", typeof(Resources))]
        NotEqual,
        [LocalizedDisplayName("RelationalOperator.Less", typeof(Resources))]
        Less,
        [LocalizedDisplayName("RelationalOperator.LessOrEqual", typeof(Resources))]
        LessOrEqual,
        [LocalizedDisplayName("RelationalOperator.Greater", typeof(Resources))]
        Greater,
        [LocalizedDisplayName("RelationalOperator.GreaterOrEqual", typeof(Resources))]
        GreaterOrEqual,
        [LocalizedDisplayName("RelationalOperator.InRange", typeof(Resources))]
        InRange,
        [LocalizedDisplayName("RelationalOperator.OutRange", typeof(Resources))]
        OutRange,
    }
}
