using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class Expression : List<ExpressionNode>
    {
        internal Expression()
        {
        }

        internal ExpressionOperand Evaluate(IRowValueAccessor accessor)
        {
            Stack<ExpressionOperand> stack = new Stack<ExpressionOperand>();

            do
            {
                foreach (ExpressionNode n in this)
                {
                    if (n is ExpressionOperand)
                        stack.Push((ExpressionOperand) n);
                    else if (n is ExpressionOperator)
                    {
                        // it's an operator
                        var oper = (ExpressionOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionOperandFromColumn)
                    {
                        var r = (ExpressionOperandFromColumn)n;
                        stack.Push(accessor.GetValue(r.ColumnName));
                    }
                    else if (n is ExpressionComparisonOperator)
                    {
                        var oper = (ExpressionComparisonOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionBooleanOperator)
                    {
                        var oper = (ExpressionBooleanOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                }
            } while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();

            string str = string.Join(',', this);
            Console.WriteLine($"{str} ==> [{result}]");

            return result;

        }

    }
}
