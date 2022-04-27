namespace JankSQL.Expressions.Functions
{
    internal class FunctionPOWER : ExpressionFunction
    {
        internal FunctionPOWER()
            : base("POWER")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            ExpressionOperand result;

            if (right.RepresentsNull || left.RepresentsNull)
                result = ExpressionOperand.NullLiteral();
            else
            {
                if (left.NodeType == ExpressionOperandType.INTEGER)
                {
                    int n = (int)Math.Pow(left.AsDouble(), right.AsDouble());
                    result = ExpressionOperand.IntegerFromInt(n);
                }
                else
                {
                    double d = Math.Pow(left.AsDouble(), right.AsDouble());
                    result = ExpressionOperand.DecimalFromDouble(d);
                }
            }

            stack.Push(result);
        }
    }
}

