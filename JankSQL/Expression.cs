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

        internal ExpressionOperand Evaluate(ResultSet resultSet, int rowIndex)
        {
            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();

            do
            {
                foreach (ExpressionNode n in this)
                {
                    if (n is ExpressionOperand)
                        stack.Push(n);
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
                        int idx = resultSet.ColumnIndex(r.ColumnName);
                        // Console.WriteLine($"Need value from {r.ColumnName}, column index {idx}");

                        ExpressionOperand[] thisRow = resultSet.Row(rowIndex);
                        ExpressionOperand val = thisRow[idx];
                        stack.Push(val);
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
            Console.WriteLine($"==> [{result}]");

            return result;

        }

    }
}
