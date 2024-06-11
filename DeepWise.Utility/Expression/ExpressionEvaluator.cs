using DeepWise.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static DeepWise.Expression.ExpressionOperations;
namespace DeepWise.Expression
{

    /// <summary>
    /// 提供計算表示式(Expression)結果的方法。
    /// </summary>
    public static class ExpressionEvaluator
    {
        static ExpressionEvaluator()
        {
            Fun.Add("Sum", FunctionCollection.Sum);
        }

        public static Dictionary<string, ExpressionFunctionHandler> Fun { get; } = new Dictionary<string, ExpressionFunctionHandler>();


        /// <summary>
        /// 解析表達式。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static object Parse(this string expression, IEvaluationContext context)
        {
            var tokens = GetTokens(expression);
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.Identifier)
                {
                    //右邊是括號(代表這是方法名稱)
                    if (i < tokens.Count - 1 && (tokens[i + 1].TokenType == TokenType.OpenParenthesis)) continue;
                    //左邊是點點(代表這是成員)
                    if (i > 0 && (tokens[i - 1].TokenType == TokenType.Dot)) continue;

                    tokens[i] = new Token(TokenType.Object, context.GetObject(tokens[i].Identifier));
                }
            }
            return Evaluate(tokens).Value;
        }

        /// <summary>
        /// 傳回確切的整數值，若非整數則會報錯。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIndex(this double value)
        {
            double diff = Math.Abs(Math.Truncate(value) - value);
            if(diff< 0.0000001|| diff > 0.9999999)
                return (int)Math.Round(value);
            else
                throw new Exception("索引值必須為整數值");
        }

        /// <summary>
        /// 取得物件的公開屬性值或<see cref="IEvaluationContext"/>介面所提供的識別項之值。
        /// </summary>
        public static object GetMember(this object target, string identifier, string keepPath = "")
        {
            if (target == null) throw new ArgumentNullException($"null 不存在成員{identifier}");
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException($"成員名稱不可為空");
            if (identifier.Contains('.'))
            {
                string pName = identifier.Split('.')[0];
                return GetMemberNative(target, pName).GetMember(identifier.Substring(identifier.IndexOf('.') + 1), Attach(pName, keepPath));
            }
            else
                return GetMemberNative(target, identifier);


            object GetMemberNative(object obj, string name)
            {
                //[New]
                if (name.IndexOf('[') is int openIndex && openIndex!=-1)
                {
                    object list_or_array = GetMemberNative(obj, name.Split('[')[0]);
                    //TODO : 目前僅支援單一索引，需再加入不規則、規則陣列的索引方法
                    int endIndex = name.GetEndParenthesesIndex(openIndex);
                    int str = openIndex + 1;
                    int end = endIndex - 1;
                    int index;
                    try
                    {
                        index = int.Parse(name.Substring(str, end - str + 1));
                    }
                    catch
                    {
                        throw new Exception("索引值必須為整數");
                    }

                    if (list_or_array is IList list)
                        return list[int.Parse(name.Substring(str, end - str + 1))];
                    else if (list_or_array is Array array)
                        return array.GetValue(int.Parse(name.Substring(str, end - str + 1)));
                    else throw new Exception($"無法套用有[]的索引至'{obj}'物件。");
                }
                //[Old]
                //if (name.Contains('['))
                //{
                //    //TODO : 目前僅支援單一索引，需再加入不規則、規則陣列的索引方法
                //    object list_or_array = GetMemberNative(obj, name.Split('[')[0]);
                //    int str = name.IndexOf('[') + 1;
                //    int end = name.IndexOf(']') - 1;
                //    int index;
                //    try
                //    {
                //        index = int.Parse(name.Substring(str, end - str + 1));
                //    }
                //    catch
                //    {
                //        throw new Exception("索引值必須為整數");
                //    }
                //    if (list_or_array is IList list)
                //        return list[int.Parse(name.Substring(str, end - str + 1))];
                //    else if (list_or_array is Array array)
                //        return array.GetValue(int.Parse(name.Substring(str, end - str + 1)));
                //    else throw new Exception($"無法套用有[]的索引至'{obj}'物件。");
                //}
                else
                {
                    if (obj is IEvaluationContext context)
                        return context.GetObject(name);
                    else
                    {
                        var pInfo = obj.GetType().GetProperty(name);
                        if (pInfo == null) throw new Exception($"在'{ keepPath }'中找不到名稱為'{ name }'的成員。");
                        return pInfo.GetValue(obj);
                    }
                }
            }

            string Attach(string current, string previous = "")
            {
                if (String.IsNullOrEmpty(previous))
                    return current;
                else
                    return previous + '.' + current;
            }
        }

        /// <summary>
        /// 獲得目標的所有的公開屬性(Brosable)名稱以及類型或<see cref="IEvaluationContext"/>介面所提供的所有識別項的名稱以及類型。
        /// </summary>
        public static Dictionary<string, Type> GetMembersInfo(this object target)
        {
            if (target is IEvaluationContext context)
            {
                return context.GetIdentifiers();
            }
            else if (target is string)
                return new Dictionary<string, Type>();
            else
            {
                var dic = new Dictionary<string, Type>();
                foreach (var item in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(IsBrowsable))
                    dic.Add(item.Name, item.PropertyType);
                return dic;
            }

            bool IsBrowsable(PropertyInfo info) => !(info.GetCustomAttribute<BrowsableAttribute>() is BrowsableAttribute att) || att.Browsable;
        }

        /// <summary>
        /// 獲得目標的所有的公開屬性(<see cref="BrowsableAttribute"/>)名稱或<see cref="IEvaluationContext"/>介面所提供的所有識別項的名稱。
        /// </summary>
        public static string[] GetMembersName(this object target) => target.GetMembersInfo().Select(x => x.Key).ToArray();

        #region Native
        static List<Token> GetTokens(string expression)
        {
            var tokens = new List<Token>();
            var reader = new Tokenizer(expression);
            while (reader.Token != TokenType.EOF)
            {
                switch (reader.Token)
                {
                    case TokenType.Identifier:
                        switch (reader.Identifier)
                        {
                            case "true":
                            case "True":
                            case "TRUE":
                                tokens.Add(new Token(TokenType.Object, true));
                                break;
                            case "false":
                            case "False":
                            case "FALSE":
                                tokens.Add(new Token(TokenType.Object, false));
                                break;
                            default:
                                tokens.Add(new Token(reader.Token, reader.Identifier));
                                break;
                        }
                        break;
                    case TokenType.Number:
                        tokens.Add(new Token(TokenType.Object, reader.Number));
                        break;
                    case TokenType.String:
                        tokens.Add(new Token(TokenType.Object, (object)reader.String));
                        break;
                    default:
                        tokens.Add(new Token(reader.Token));
                        break;
                }
                reader.NextToken();
            }
            return tokens;
        }

        static readonly TokenType[] comparisons = new TokenType[] { TokenType.Greater, TokenType.GreaterOrEqual, TokenType.Less, TokenType.LessOrEqual, TokenType.Equal, TokenType.NotEqual };
        static Token Evaluate(List<Token> tokens, string funcName = "")
        {
            //Function & Parentheses===========================================================================
            while (TryGetToken(tokens, TokenType.OpenParenthesis, out int open))
            {
                int end = GetEndParenthesesIndex(tokens, open);
                if (open > 0 && tokens[open - 1].TokenType == TokenType.Identifier)
                {
                    int str = open;
                    string funName = tokens[open - 1].Identifier;
                    var value = Evaluate(tokens.GetRange(open + 1, end - open - 1), funName);
                    tokens.RemoveRange(open - 1, end - open + 2);
                    tokens.Insert(open - 1, value);
                }
                else
                {
                    int str = open;
                    var value = Evaluate(tokens.GetRange(str + 1, end - str - 1));
                    tokens.RemoveRange(str, end - str + 1);
                    tokens.Insert(str, value);
                }
            }
            //Member & indexor=================================================================================
            while (TryGetTokenAny(tokens, out int open,out TokenType match, new TokenType[] { TokenType.Dot , TokenType.OpenSquareBracket }))
            {
                switch (match)
                {
                    case TokenType.Dot:
                        {
                            if (tokens[open + 1].TokenType == TokenType.Identifier)
                            {
                                object target = tokens[open - 1].Value;
                                object value = target.GetMember(tokens[open + 1].Identifier);
                                tokens.RemoveRange(open - 1, 3);
                                tokens.Insert(open - 1, new Token(TokenType.Object, value));
                                break;
                            }
                            else
                                throw new Exception();
                        
                        }
                    case TokenType.OpenSquareBracket:
                        {
                            int end = GetEndParenthesesIndex(tokens, open);
                            var target = tokens[open - 1];
                            if (open > 0 && target.TokenType == TokenType.Object)
                            {
                                int str = open;
                                if (target.Value is Array array)
                                {
                                    var index = ((double)Evaluate(tokens.GetRange(open + 1, end - open - 1)).Value).ToIndex();
                                    tokens.RemoveRange(open - 1, end - open + 2);
                                    tokens.Insert(open - 1, new Token(TokenType.Object, array.GetValue(index)));
                                }
                                else if (target.Value is IList list)
                                {
                                    var index = ((double)Evaluate(tokens.GetRange(open + 1, end - open - 1)).Value).ToIndex();
                                    tokens.RemoveRange(open - 1, end - open + 2);
                                    tokens.Insert(open - 1, new Token(TokenType.Object, list[index]));
                                }
                                else if (target.Value is IDictionary dic)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                    throw new NotImplementedException();
                                break;
                            }
                            else
                                throw new Exception();
                        }
                }
            }
            //Arithmetic=======================================================================================
            while (TryGetToken(tokens,TokenType.Pow, out int index)) EvaluateOperator(tokens, index);

            while (TryGetToken(tokens,TokenType.Mod, out int index)) EvaluateOperator(tokens, index);

            while (TryGetToken(tokens,TokenType.Multiply, out int index)) EvaluateOperator(tokens, index);
            while (TryGetToken(tokens, TokenType.Divide, out int index)) EvaluateOperator(tokens, index);
            while (TryGetTokenAny(tokens, out int index, out TokenType matched, new TokenType[] { TokenType.Add, TokenType.Subtract })) EvaluateOperator(tokens, index);
            //Comparison=======================================================================================
            while (TryGetTokenAny(tokens, out int index, out TokenType matched, comparisons)) EvaluateOperator(tokens, index);
            //LogicalOperation=================================================================================
            while (TryGetToken(tokens,TokenType.And, out int index)) EvaluateOperator(tokens, index);
            while (TryGetToken(tokens,TokenType.Or, out int index)) EvaluateOperator(tokens, index);
            //Finction=========================================================================================
            if (!string.IsNullOrEmpty(funcName))
            {
                if (tokens.Count == 1)
                {
                    if (tokens[0].Value is Array array)
                        return new Token(TokenType.Object, GetFunction(funcName)(array));
                    else
                        return new Token(TokenType.Object, GetFunction(funcName)(new object[] { tokens[0].Value }));
                }
                else
                {
                    //TODO : seperate comma
                    return new Token(TokenType.Object, GetFunction(funcName)(tokens.Where(x => x.TokenType != TokenType.Comma).Select(x=>x.Value).ToArray()));
                }
            }

            //Result
            return tokens.Count == 1 ? tokens.First() : throw new Exception("Expression Parsing Error");
    
        }
        [DebuggerStepThrough]
        static bool TryGetToken(List<Token> tokens, TokenType tokenType,out int index)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == tokenType)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        [DebuggerStepThrough]
        static bool TryGetTokenAny(List<Token> tokens, out int index, out TokenType matchToken, params TokenType[] tokenTypes)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokenTypes.Contains(tokens[i].TokenType))
                {
                    index = i;
                    matchToken = tokens[i].TokenType;
                    return true;
                }
            }
            index = -1;
            matchToken = default;
            return false;
        }
        [DebuggerStepThrough]
        static void EvaluateOperator(List<Token> tokens, int operatorIndex)
        {
            TokenType token = tokens[operatorIndex].TokenType;
            Func<object, object, object> func = token.GetOperator();
            if (operatorIndex != -1)
            {
                if (operatorIndex == 0 && (token == TokenType.Subtract || token == TokenType.Add) && tokens[operatorIndex + 1].TokenType == TokenType.Object)
                {
                    object b = tokens[operatorIndex + 1].Value;
                    object a = Activator.CreateInstance(b.GetType());
                    var result = func(a, b);
                    if (result != null)
                    {
                        tokens.RemoveRange(operatorIndex, 2);
                        tokens.Insert(operatorIndex, new Token(TokenType.Object, result));
                    }
                }
                else if (tokens[operatorIndex - 1].TokenType == TokenType.Object && tokens[operatorIndex + 1].TokenType == TokenType.Object)
                {
                    object a = tokens[operatorIndex - 1].Value;
                    object b = tokens[operatorIndex + 1].Value;
                    var result = func(a, b);
                    if (result != null)
                    {
                        tokens.RemoveRange(operatorIndex - 1, 3);
                        tokens.Insert(operatorIndex - 1, new Token(TokenType.Object, result));
                    }
                }
                else
                    throw new Exception(token.ToString() + " fail");
            }
        }
        [DebuggerStepThrough]
        static int GetEndParenthesesIndex(List<Token> tokens,int openParensIndex)
        {
            int count = 1;
            TokenType close;
            TokenType open = tokens[openParensIndex].TokenType;
            switch (open)
            {
                case TokenType.OpenParenthesis: close = TokenType.CloseParenthesis; break;
                case TokenType.OpenSquareBracket: close = TokenType.CloseSquareBracket; break;
                default:
                    throw new Exception("token[" + openParensIndex + "] is not an open-parens");
            }
            for (int i = openParensIndex + 1; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == open)
                    count++;
                else if (tokens[i].TokenType == close)
                    count--;
                if (count == 0) return i;
            }
            throw new Exception("can't not find the matched close-parens");
        }
        [DebuggerStepThrough]
        static int GetEndParenthesesIndex(this string expression, int openParensIndex)
        {
            int count = 1;
            char close;
            char open = expression[openParensIndex];
            switch (open)
            {
                case '[': close = ']'; break;
                case '(': close = ')'; break;
                case '{': close = '}'; break;
                case '<': close = '>'; break;
                default:
                    throw new Exception($"{open} isn't a open-parenthesis");
            }
            for (int i = openParensIndex + 1; i < expression.Length; i++)
            {
                if (expression[i] == open)
                    count++;
                else if (expression[i] == close)
                    count--;
                if (count == 0) return i;
            }
            throw new Exception("can't not find the matched close-parenthesis");
        }
        #endregion
    }

    public delegate object ExpressionFunctionHandler(params object[] parameters);

    public static class FunctionCollection
    {
        public static object Sum(params object[] parameters)
        {
            if (parameters.Length == 0) throw new ArgumentException();
            var type = parameters[0].GetType();
            switch (type.Name)
            {
                case nameof(Double):
                    return parameters.Cast<double>().Sum();
                case nameof(Point):
                    var p2coll = parameters.Cast<Point>();
                    return new Point(p2coll.Sum(p => p.X), p2coll.Sum(p => p.Y));
                case nameof(Point3D):
                    var p3coll = parameters.Cast<Point3D>();
                    return new Point3D(p3coll.Sum(p => p.X), p3coll.Sum(p => p.Y), p3coll.Sum(p => p.Z));
                default: throw new NotImplementedException("未支援型別" + type.Name + "的Sum操作");
            }
        }
    }

    public static class ExpressionHelper
    {
        public static string IdentifierToCodeName(string expression, IEvaluationContext context)
        {
            if (expression is null) return null;
            var codeName = new Dictionary<string, string>();
            foreach (var name in context.GetMembersName())
            {
                try
                {
                    if (context.GetObject(name) is IIdentifierCodeName identifier)
                        codeName.Add(name, identifier.CodeName);
                }
                catch { }
            }
            //string s = "";

            //[New]
            foreach(var item in codeName.Keys)
            {
                expression = expression.Replace(item, codeName[item]);
            }
            return expression;
            //[Old]
            var builder = new StringBuilder();
            string previous = "";
            foreach (string item in expression.SplitKeep(TokenHelper.EscapeChar))
            {

                if (previous != "." //判斷是否為第一個成員
                    && codeName.ContainsKey(item))
                {
                    string newItem = item.Remove(0, item.Length);
                    newItem = newItem.Insert(0, codeName[item]);
                    builder.Append(newItem);
                }
                else
                    builder.Append(item);
            }
            return builder.ToString();
        }
        public static string CodeNameToIdentifier(string expression, IEvaluationContext context)
        {
            if (expression == null) return null;
            var codeName = new Dictionary<string, string>();
            foreach (var name in context.GetMembersName())
            {
                try
                {
                    if (context.GetObject(name) is IIdentifierCodeName identifier)
                        codeName.Add(identifier.CodeName, name);
                }
                catch { }
            }
            //string s = "";
            var builder = new StringBuilder();
            string previous = "";
            foreach (string item in expression.SplitKeep(TokenHelper.EscapeChar))
            {

                if (previous != "." //判斷是否為第一個成員
                    && codeName.ContainsKey(item))
                {
                    string newItem = item.Remove(0, item.Length);
                    newItem = newItem.Insert(0, codeName[item]);
                    builder.Append(newItem);
                }
                else
                    builder.Append(item);
            }
            return builder.ToString();
        }
        [DebuggerStepThrough]
        public static string GetName(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`")));
            sb.Append(t.GetGenericArguments().Aggregate("<",

                delegate (string aggregate, Type type)
                {
                    return aggregate + (aggregate == "<" ? "" : ",") + GetName(type);
                }
                ));
            sb.Append(">");
            return sb.ToString();
        }
        [DebuggerStepThrough]
        public static IEnumerable<string> SplitKeep(this string s, params char[] delims)
        {
            int start = 0, index;
            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }
    }
}
 