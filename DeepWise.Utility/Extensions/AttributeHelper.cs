using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class AttributeHelper
    {
        public static bool TryGetCustomAttribute<T>(this MemberInfo element, bool inherit, out T attribute) where T : Attribute
        {
            attribute = element.GetCustomAttribute<T>(inherit);
            return attribute != null;
        }

        public static bool TryGetCustomAttribute<T>(this MemberInfo element, out T attribute) where T : Attribute
        {
            attribute = element.GetCustomAttribute<T>(true);
            return attribute != null;
        }

        public static bool TryGetCustomAttributes<T>(this MemberInfo element, bool inherit, out IEnumerable<T> attributes) where T : Attribute
        {
            attributes = element.GetCustomAttributes<T>(inherit);
            return attributes.Count() > 0;
        }
    }
}
