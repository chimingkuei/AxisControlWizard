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
namespace DeepWise.Expression
{
    public static class DynamicExpressionExtensions
    {
        public static bool IsList(this IDynamicExpression expression)
        {
            return expression.ValueType.IsGenericType
                && expression.ValueType.GetGenericTypeDefinition() == typeof(List<>);
        }
        public static bool IsList(this IVariable variable)
        {
            return variable.ValueType.IsGenericType
                && variable.ValueType.GetGenericTypeDefinition() == typeof(List<>);
        }
    }

    

}
