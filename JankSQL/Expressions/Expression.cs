
namespace JankSQL
{
    internal class Expression : List<ExpressionNode>
    {
        internal Expression()
        {
        }

        internal ExpressionOperand Evaluate(IRowValueAccessor? accessor)
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
                        if (accessor == null)
                            throw new ExecutionException("Not in a row context to evaluate {this}");

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
            Console.WriteLine($"{this} ==> [{result}]");

            return result;
        }

        public override string ToString()
        {
            string str = string.Join(',', this);
            return str;
        }


    }
}
