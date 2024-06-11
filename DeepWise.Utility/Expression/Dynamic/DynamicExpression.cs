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
using DeepWise.Controls;
using DeepWise.Expression.Dynamic;

namespace DeepWise.Expression
{
    /// <summary>
    /// 表示巢狀式運算結構，透過<see cref="Evaluate"/>計算其結果。
    /// </summary>
    [JsonConverter(typeof(DynamicExpressionJsonConverter)),CustomEditor(typeof(DynamicValueEditor))]
    public interface IDynamicExpression
    {
        Constructor Constructor { get; set; }

        object Evaluate(IEvaluationContext pnl);
        void Edit(IEvaluationContext pnl, IList list);

        Type ValueType { get; }
        IDynamicExpression[] References { get; set; }
        string[] Arguments { get; set; }

        string ToString(IEvaluationContext context);
    }

    /// <summary>
    /// 表示巢狀式運算結構，透過<see cref="Evaluate"/>計算其結果。
    /// </summary>
    /// <typeparam name="T">結果的類型</typeparam>
    [JsonConverter(typeof(DynamicExpressionJsonConverter))]
    public class DynamicExpression<T> : IDynamicExpression
    {
        public override string ToString()
        {
            string tmp = "";
            switch (Constructor)
            {
                case Constructor.None:
                    break;
                case Constructor.Expression:
                case Constructor.Variable:
                case Constructor.RealNumber:
                case Constructor.BooleanValue:
                case Constructor.String:
                    tmp = Arguments[0];
                    break;
                case Constructor.ListContains:
                    tmp = $"{Arguments[0]}包含項目{References[0]}";
                    break;
                case Constructor.ListCount:
                    tmp = $"{Arguments[0]} 的項目數量";
                    break;
                case Constructor.Conditional:
                    tmp = $"若條件{References[0]}成立則{References[1]}否則{References[2]}";
                    break;
                case Constructor.Add:
                    tmp = $"加入{References[0]}";
                    break;
                case Constructor.RemoveAt:
                    tmp = $"移除第{References[0]}個項目";
                    break;
                case Constructor.Insert:
                    tmp = $"將項目{References[0]}插入{References[1]}的位置";
                    break;
                case Constructor.Clear:
                    tmp = "清空列表";
                    break;
                default:
                    try
                    {
                        var enumInfo = typeof(Constructor).GetMember(Constructor.ToString())[0];
                        var atts = enumInfo.GetCustomAttributes(typeof(ConstructorInfoAttribute), false);
                        var att = (ConstructorInfoAttribute)atts.FirstOrDefault();
                        string s = att.Description;
                        for (int i = 0; i < att.Parameters.Length; i++)
                        {
                            s = s.Replace("{" + i + "}", References[i] != null ? References[i].ToString() : "null");
                        }
                        s = s.Replace("\n", "");
                        tmp = att.ShowParentheses ? $"({s})" : s;
                    }
                    catch 
                    { 
                        tmp = Constructor.ToString(); 
                    };
                    break;
            }
            if (String.IsNullOrWhiteSpace(tmp))
                return "null";
            else
                return tmp;
        }
        public string ToString(IEvaluationContext context)=> ExpressionHelper.IdentifierToCodeName(this.ToString(), context);

        [JsonIgnore]
        public Constructor Constructor
        {
            get => _contructor;
            set
            {
                if (_contructor == value) return;
                var enumInfo = typeof(Constructor).GetMember(value.ToString())[0];
                var atts = enumInfo.GetCustomAttributes(typeof(ConstructorInfoAttribute), false);
                var att = (ConstructorInfoAttribute)atts.FirstOrDefault();
                if (att!=null && !att.IsTypeAvilible(typeof(T))) throw new Exception("Constructor類型錯誤");

                atts = enumInfo.GetCustomAttributes(typeof(ParametersInfoAttribute), false);
                var argsInfo = (ParametersInfoAttribute)atts.FirstOrDefault();
                if (argsInfo == null) throw new Exception($"尚未標記{nameof(DeepWise.Expression.Constructor)}.{value} 的 {nameof(ParametersInfoAttribute)}");
                References = new IDynamicExpression[argsInfo.ReferencesCount];
                Arguments = new string[argsInfo.ArgumentsCount];
                _contructor = value;
                return;
            }
        }

        public IDynamicExpression[] References { get; set; } = new IDynamicExpression[0];
        public string[] Arguments { get; set; } = new string[0];
  
        public T Evaluate(IEvaluationContext pnl)
        {
            switch (Constructor)
            {
                case Constructor.Expression:
                    return (T)ExpressionEvaluator.Parse(Arguments[0], pnl);
                case Constructor.RealNumber:
                    return (T)(object)Double.Parse(Arguments[0]);
                case Constructor.BooleanValue:
                    return (T)(object)Boolean.Parse(Arguments[0]);
                case Constructor.String:
                    return (T)(object)Arguments[0];
                case Constructor.Variable:
                    return (T)pnl.GetMember(Arguments[0]);       
                case Constructor.ListCount:
                    return (T)(object)Convert.ToDouble((pnl.GetMember(Arguments[0]) as IList).Count);
                case Constructor.ListContains:
                    return (T)(object)(pnl.GetObject(Arguments[0]) as IList).Contains(References[0].Evaluate(pnl));
                case Constructor.Conditional:
                    return (bool)References[0].Evaluate(pnl) ? (T)References[1].Evaluate(pnl) : (T)References[2].Evaluate(pnl);
                default:
                    {
                        var enumInfo = typeof(Constructor).GetMember(Constructor.ToString())[0];
                        var att = (ConstructorInfoAttribute)enumInfo.GetCustomAttributes(typeof(ConstructorInfoAttribute), false).FirstOrDefault();
                        return (T)att.Func(References.Select(x => x.Evaluate(pnl)).ToArray());
                    }
                case Constructor.Add:
                case Constructor.AddRange:
                case Constructor.RemoveAt:
                case Constructor.Insert:
                case Constructor.Clear:
                    throw new Exception();
            }
        }
        public void Edit(IEvaluationContext pnl, IList list)
        {
            switch (Constructor)
            {
                case Constructor.Add:
                    list.Add(References[0].Evaluate(pnl));
                    break;
                case Constructor.AddRange:
                    foreach (var item in ((IEnumerable)References[0].Evaluate(pnl)).Cast<object>().ToArray())
                        list.Add(item);
                    break;
                case Constructor.Insert:
                    list.Insert(Convert.ToInt32(References[1].Evaluate(pnl)), References[0].Evaluate(pnl));
                    break;
                case Constructor.RemoveAt:
                    list.RemoveAt(Convert.ToInt32(References[0].Evaluate(pnl)));
                    break;
                case Constructor.Remove:
                    list.Remove(References[0].Evaluate(pnl));
                    break;
                case Constructor.Clear:
                    list.Clear();
                    break;
                default: throw new NotImplementedException();
            }
        }

        [JsonProperty(nameof(Constructor))]
        Constructor _contructor;
        object IDynamicExpression.Evaluate(IEvaluationContext pnl) => Evaluate(pnl);
        Type IDynamicExpression.ValueType => typeof(T);
    }

    public class DynamicExpressionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDynamicExpression).IsAssignableFrom(objectType);
        }
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var valueObj = JObject.Load(reader);
            Type type = typeof(DynamicExpression<>).MakeGenericType(valueObj.GetValue("Type").ToObject<Type>());
            var dv = Activator.CreateInstance(type) as IDynamicExpression;
            dv.Constructor = (Constructor)Enum.Parse(typeof(Constructor), valueObj.GetValue(nameof(IDynamicExpression.Constructor)).ToObject<String>());
            if (valueObj.ContainsKey(nameof(IDynamicExpression.References)))
                dv.References = valueObj.GetValue(nameof(IDynamicExpression.References)).ToObject<IDynamicExpression[]>();
            if (valueObj.ContainsKey(nameof(IDynamicExpression.Arguments)))
                dv.Arguments = valueObj.GetValue(nameof(IDynamicExpression.Arguments)).ToObject<string[]>();
            return dv;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dv = value as IDynamicExpression;
            var jObj = new JObject();
            jObj.Add("Type", JToken.FromObject(value.GetType().GenericTypeArguments[0]));
            jObj.Add(nameof(IDynamicExpression.Constructor), JToken.FromObject(dv.Constructor.ToString()));
            if (dv.References.Length > 0)
                jObj.Add(nameof(IDynamicExpression.References), JToken.FromObject(dv.References));
            if (dv.Arguments.Length > 0)
                jObj.Add(nameof(IDynamicExpression.Arguments), JToken.FromObject(dv.Arguments));
            jObj.WriteTo(writer);
        }
    }
}
