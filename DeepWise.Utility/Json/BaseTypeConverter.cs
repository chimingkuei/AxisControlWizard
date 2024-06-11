using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Json
{
    public class BaseTypeConverter<T> : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter<T>() };
        public override bool CanConvert(Type objectType)
        {
            return (typeof(T).IsAssignableFrom(objectType));
        }
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObj = JObject.Load(reader);
            Type type = jObj[ObjectTypeKey].ToObject<Type>();
            return JsonConvert.DeserializeObject(jObj.ToString(), type, SpecifiedSubclassConversion);
        }

        const string ObjectTypeKey = "__ObjectType";
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObj = new JObject();
            jObj.Add(ObjectTypeKey, JToken.FromObject(value.GetType()));
            jObj.Merge(JObject.FromObject(value,new JsonSerializer() {ContractResolver = SpecifiedSubclassConversion.ContractResolver }));
            jObj.WriteTo(writer);
        }
    }

    internal class BaseSpecifiedConcreteClassConverter<T> : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(T).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
}
