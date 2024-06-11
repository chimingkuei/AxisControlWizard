using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    public static class MotionExtension
    {
        public static void Variable_Bit_Set(this ref int target, int n_Bit, int value)
        {
            if (value == 1)
                target |= (1 << n_Bit);
            else
                target &= (~(1 << n_Bit));
        }
        public static int Variable_Bit_Get(this int Variable, int n_Bit) => (Variable & (1 << n_Bit)) >> n_Bit;
        /// <summary>
        /// ex. (Value 0b0010) set to (Variable 0b00000100 4~7bit(StartBit=4,Count=4) ) => 0b00100100
        /// </summary>
        /// <param name="Variable"></param>
        /// <param name="StartBit"></param>
        /// <param name="Count"></param>
        /// <param name="Value"></param>
        public static void Variable_Multi_Bit_Set(this ref int Variable, int StartBit, int Count, int Value)
        {
            for (int i = StartBit; i < StartBit + Count; i++)
            {
                Variable_Bit_Set(ref Variable, i, Variable_Bit_Get(Value, i - StartBit));
            }
        }
    }
}
