using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    public class MotionValidator
    {
        public ValidatorType ConditionType { get; set; }

        public bool SetterBooleanCondition { get; set; }
        public DeepWise.NumericalComparison<double> SetterNumericCondition { get; set; }

        public int TargetAxisID { get; set; }
        public IOPortInfo TargetIOPort { get; set; }
        public bool TargetBoolValue { get; set; }
        public DeepWise.NumericalComparison<double> TargetNumericRange { get; set; }


        public bool Validate(ADLINK_Motion cntlr, object value)
        {
            //先判斷要設定的值是否監聽
            if (value is bool b)
            {
                if (SetterBooleanCondition != b) return true;
            }
            else if (value is double f)
            {
                if (!SetterNumericCondition.Predicate(f)) return true;

            }

            //========================================================
            switch (ConditionType)
            {
                case ValidatorType.DIO:
                    return cntlr.GetIOValue<bool>(TargetIOPort) == TargetBoolValue;
                case ValidatorType.AIO:
                    return TargetNumericRange.Predicate(cntlr.GetIOValue<double>(TargetIOPort));
                case ValidatorType.AxisPosition:
                    return TargetNumericRange.Predicate(cntlr[TargetAxisID].Position);
                default:
                    throw new NotImplementedException();
            }
        }


    }

    public enum ValidatorType
    {
        DIO, AIO, AxisPosition
    }

    public class IOOperationInvalidException : Exception
    {
        public IOOperationInvalidException(string port) : base($"Set I\\O {port} fali")
        {

        }

    }
}
