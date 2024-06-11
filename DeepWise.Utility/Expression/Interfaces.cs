using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DeepWise.Expression
{
    /// <summary>
    /// 表示提供Expression運算的識別項的內文。
    /// </summary>
    public interface IEvaluationContext
    {
        object GetObject(string name);
        Dictionary<string, Type> GetIdentifiers();
    }


    /// <summary>
    /// 表示為作為<see cref="ExpressionEvaluator"/>的識別項時，提供簡易的顯示名稱。
    /// </summary>
    public interface IIdentifierCodeName
    {
        string CodeName { get; }
    }
}