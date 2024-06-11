using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Newtonsoft.Json
{
    public static class JsonExtensions
    {
        public static object JsonClone(this object obj)
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj), obj.GetType());
        }
    }
}
