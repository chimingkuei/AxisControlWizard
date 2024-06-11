using DeepWise.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Expression
{
    public static class ExpressionOperations
    {
        public static Func<Array,object> GetFunction(string name)
        {
            var funcInfo = typeof(ExpressionOperations).GetMethod(name);
            //var funcInfo = typeof(ExpressionOperations).GetMethod(char.ToUpper(name[0]) + name.ToLower().Substring(1));
            try
            {
                return (Func<Array, object>)funcInfo.CreateDelegate(typeof(Func<Array, object>));
            }
            catch (Exception ex)
            {
                throw new NotImplementedException($"找不到方法: '{name}'");
            }
        }
        public static object Sum(Array array)
        {
            var type = array.GetType().GetElementType();
            switch (type.Name)
            {
                case nameof(Object):
                    return Sum(array.Cast(array.GetValue(0).GetType()));
                case nameof(Double):
                    return (array as IEnumerable<double>).Sum();
                case nameof(Point):
                    var p2coll = array as IEnumerable<Point>;
                    return new Point(p2coll.Sum(p => p.X), p2coll.Sum(p => p.Y));
                case nameof(Point3D):
                    var p3coll = array as IEnumerable<Point3D>;
                    return new Point3D(p3coll.Sum(p => p.X), p3coll.Sum(p => p.Y), p3coll.Sum(p => p.Z));
                default: throw new NotImplementedException("未支援型別" + type.Name + "的合併操作");
            }
        }
        public static object Avg(Array array)
        {
            if (array.Length == 0) throw new Exception("Avg運算之目標集合不可為空");
            var type = array.GetType().GetElementType();
            switch (type.Name)
            {
                case nameof(Object):
                    var c = array as IEnumerable<object>;
                    if (c.Count() > 0) 
                        return Avg(array.Cast(array.GetValue(0).GetType()));
                    else
                        throw new Exception("集合不可為空");
                case nameof(Double):
                    return (array as IEnumerable<double>).Average();
                case nameof(Point):
                    var p2coll = array as IEnumerable<Point>;
                    return new Point(p2coll.Average(p => p.X), p2coll.Average(p => p.Y));
                case nameof(Point3D):
                    var p3coll = array as IEnumerable<Point3D>;
                    return new Point3D(p3coll.Average(p => p.X), p3coll.Average(p => p.Y), p3coll.Average(p => p.Z));
                default: throw new NotImplementedException("未支援型別" + type.Name + "的平均值操作");
            }
        }
        public static object Max(Array array)
        {
            if(array.Length > 0)
            {
                try
                {
                    return array.Cast<IComparable>().Max();
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的最大值操作");
                }
            }
            else
                throw new Exception("集合不可為空");
        }
        public static object Min(Array array)
        {

            if (array.Length > 0)
            {
                try
                {
                    return array.Cast<IComparable>().Min();
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的最小值操作");
                }
            }
            else
                throw new Exception("集合不可為空");
        }
        public static object Sin(Array array)
        {
            switch (array.Length)
            {
                case 1:
                    try
                    {
                        return Math.Sin((double)array.GetValue(0));
                    }
                    catch (Exception ex)
                    {
                        throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的正弦函數操作");
                    }
                case 0:
                    throw new Exception("集合不可為空");
                default:
                    throw new Exception("Sin函數引述輸入錯誤");

            }
        }
        public static object Cos(Array array)
        {
            switch (array.Length)
            {
                case 1:
                    try
                    {
                        return Math.Cos((double)array.GetValue(0));
                    }
                    catch (Exception ex)
                    {
                        throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的餘弦函數操作");
                    }
                case 0:
                    throw new Exception("集合不可為空");
                default:
                    throw new Exception("Cos函數引述輸入錯誤");

            }
        }
        public static object Tan(Array array)
        {
            switch (array.Length)
            {
                case 1:
                    try
                    {
                        return Math.Tan((double)array.GetValue(0));
                    }
                    catch (Exception ex)
                    {
                        throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的正切函數操作");
                    }
                case 0:
                    throw new Exception("集合不可為空");
                default:
                    throw new Exception("Tan函數引述輸入錯誤");

            }
        }
        public static object IsNaN(Array array)
        {
            switch (array.Length)
            {
                case 1:
                    var value = array.GetValue(0);
                    var method = value.GetType().GetMethod("IsNaN", BindingFlags.Public | BindingFlags.Static);
                    if (method != null)
                        return method.Invoke(null, new object[] { value });
                    else
                        throw new NotImplementedException("未支援型別" + array.GetType().GetElementType().Name + "的IsNaN函數操作");
                default:
                    throw new Exception("IsNaN函數僅能有一個引述值");
            }
        }
        //==============================================================================
        public static Func<object, object, object> GetOperator(this char symbol)
        {
            switch (symbol)
            {
                case '*': return Multiply;
                case '/': return Divide;
                case '+': return Add;
                case '-': return Subtract;
                case '=': return (a, b) => Object.Equals(a, b);
                case '≠': return (a, b) => !Object.Equals(a, b);
                case '>': return (a, b) => (a as IComparable).CompareTo(b) == 1;
                case '<': return (a, b) => (a as IComparable).CompareTo(b) == -1;
                case '≥': return (a, b) => (a as IComparable).CompareTo(b) >= 0;
                case '≤': return (a, b) => (a as IComparable).CompareTo(b) <= 0;
                case '&': return (a, b) => (bool)a && (bool)b;
                case '|': return (a, b) => (bool)a || (bool)b;
                default: throw new Exception($"\"{symbol}\" is not an operator");
            }
        }
        public static Func<object, object, object> GetOperator(this TokenType token)
        {
            switch (token)
            {
                case TokenType.Multiply:       return Multiply;
                case TokenType.Divide:         return Divide;
                case TokenType.Add:            return Add;
                case TokenType.Mod:            return Mod;
                case TokenType.Pow:            return Pow;
                case TokenType.Subtract:       return Subtract;
                case TokenType.Equal:          return (a, b) => Object.Equals(a, b);
                case TokenType.NotEqual:       return (a, b) => !Object.Equals(a, b);
                case TokenType.Greater:        return (a, b) => (a as IComparable).CompareTo(b) == 1;
                case TokenType.Less:           return (a, b) => (a as IComparable).CompareTo(b) == -1;
                case TokenType.GreaterOrEqual: return (a, b) => (a as IComparable).CompareTo(b) >= 0;
                case TokenType.LessOrEqual:    return (a, b) => (a as IComparable).CompareTo(b) <= 0;
                case TokenType.And:            return (a, b) => (bool)a && (bool)b;
                case TokenType.Or:             return (a, b) => (bool)a || (bool)b;
                default: throw new Exception("token is not an operator");
            }
        }

        public static object Add(object a, object b)
        {
            if (a is string || b is string) return a.ToString() + b.ToString();
            if (a is double va && b is double vb)
                return va + vb;
            else if (a is Point pa && b is Point pb)
                return pa + (Vector)pb;
            else if (a is Point3D p3a && b is Point3D p3b)
                return p3a + (Vector3D)p3b;
            else if (a is Point6 p6a && b is Point6 p6b)
                return new Point6(p6a.X + p6b.X, p6a.Y + p6b.Y, p6a.Z + p6b.Z, p6a.A + p6b.A, p6a.B + p6b.B, p6a.C + p6b.C);
            else
            {
                var ta = a.GetType();
                var tb = b.GetType();
                MethodInfo info = ta.GetMethod("op_Addition", new Type[] { ta, tb });
                if(info ==null)info = tb.GetMethod("op_Addition", new Type[] { ta, tb });
                if (info != null)
                    return info.Invoke(null, new object[] { a, b });
                else
                    throw new Exception($"加法運算不可套用至類型為 '{a.GetType()}' 和 '{b.GetType()}' 的運算元");
            }
        }
        public static object Mod(object a, object b)
        {
            if (a is double va && b is double vb)
                return va % vb;
            else
                throw new Exception($"加法運算不可套用至類型為 '{a.GetType()}' 和 '{b.GetType()}' 的運算元");
        }
        public static object Pow(object a, object b)
        {
            if (a is double va && b is double vb)
                return Math.Pow(va,vb);
            else
                throw new Exception($"加法運算不可套用至類型為 '{a.GetType()}' 和 '{b.GetType()}' 的運算元");
        }
        public static object Subtract(object a, object b)
        {
            if (a is double va && b is double vb)
                return va - vb;
            else if (a is Point pa && b is Point pb)
                return pa - (Vector)pb;
            else if (a is Point3D p3a && b is Point3D p3b)
                return p3a - (Vector3D)p3b;
            else
                throw new Exception($"減法運算不可套用至類型為 '{a.GetType()}' 和 '{b.GetType()}' 的運算元");
        }
        public static object Multiply(object a, object b)
        {
            if (a is double scaler)
            {
                if (b is double v)
                    return scaler * v;
                else if (b is Point p)
                    return new Point(scaler * p.X, scaler * p.Y);
                else if (b is Point3D p3)
                    return new Point3D(scaler * p3.X, scaler * p3.Y, scaler * p3.Z);
                else if (b is Point6 p6)
                    return new Point6(scaler * p6.X, scaler * p6.Y, scaler * p6.Z, scaler * p6.A, scaler * p6.B, scaler * p6.C);
                
            }
            else if (b is double scaler2)
            {
                if (a is double v)
                    return scaler2 * v;
                else if (a is Point p)
                    return new Point(scaler2 * p.X, scaler2 * p.Y);
                else if (a is Point3D p3)
                    return new Point3D(scaler2 * p3.X, scaler2 * p3.Y, scaler2 * p3.Z);
                else if (a is Point6 p6)
                    return new Point6(scaler2 * p6.X, scaler2 * p6.Y, scaler2 * p6.Z, scaler2 * p6.A, scaler2 * p6.B, scaler2 * p6.C);
            }
            throw new NotImplementedException("未支援" + a.GetType().Name + "類型與" + b.GetType().Name + "類型的乘法操作");
        }
        public static object Divide(object a, object b)
        {
            if(b is double divsor) 
            {
                if (a is double v)
                    return v / divsor;
                else if (a is Point p)
                    return new Point(p.X / divsor, p.Y / divsor);
                else if (a is Point3D p3)
                    return new Point3D(p3.X / divsor, p3.Y / divsor, p3.Z / divsor);
                else if (a is Point6 p6)
                    return new Point6(p6.X / divsor, p6.Y / divsor, p6.Z / divsor, p6.A / divsor, p6.B / divsor, p6.C / divsor);
                else
                    throw new Exception($"類型 '{a.GetType()}' 無法套用除法運算");
            }
            else
                throw new Exception($"除法運算不可套用至類型為 '{a.GetType()}' 和 '{b.GetType()}' 的運算元");

        }

        public static Array Cast(this Array coll, Type type)
        {
            var array = Array.CreateInstance(type, coll.Length);
            int index = 0;
            foreach (var item in coll) array.SetValue(item, index++);
            return array;
        }
    }
}