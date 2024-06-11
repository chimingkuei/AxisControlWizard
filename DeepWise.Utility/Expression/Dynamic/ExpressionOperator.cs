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
    public interface IExpressionOperator
    {
        void Operate(IEvaluationContext context);
    }

    public enum ListOperationAction { Add, AddRange, Insert, Remove, RemoveAt, Clear }

    public class ListOperation : IExpressionOperator
    {
        public void Operate(IEvaluationContext context)
        {
            if (!TargetList.IsList()) throw new Exception();
            var list = TargetList.Evaluate(context) as IList;
            switch (Action)
            {
                case ListOperationAction.Add:
                    list.Add(Value.Evaluate(context));
                    break;
                case ListOperationAction.AddRange:
                case ListOperationAction.Insert:
                case ListOperationAction.Remove:
                case ListOperationAction.RemoveAt:
                    break;
                case ListOperationAction.Clear:
                    break;
            }
        }

        public IDynamicExpression TargetList { get; set; }
        public IDynamicExpression Value { get; set; }
        public ListOperationAction Action { get; set; }
    }
    public class VariableAssigner : IExpressionOperator
    {
        public void Operate(IEvaluationContext context)
        {
            if (Target.ValueType != NewValue.ValueType) throw new Exception();
            if (!Target.IsList())
                Target.Value = NewValue.Evaluate(context);
            else
            {
                var list = Target.Value as IList;
                list.Clear();
                foreach (var item in NewValue.Evaluate(context) as IList)
                    list.Add(item);
            }
        }
        public IVariable Target { get; set; }
        public IDynamicExpression NewValue { get; set; }
    }
}
