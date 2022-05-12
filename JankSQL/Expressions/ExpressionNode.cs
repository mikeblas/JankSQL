namespace JankSQL.Expressions
{
    public abstract class ExpressionNode
    {
        public static bool TypeFromString(string str, out ExpressionOperandType operandType)
        {
            if (str.Equals("INTEGER", StringComparison.OrdinalIgnoreCase) || str.Equals("INT", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.INTEGER;
            }
            else if (str.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase) || str.Equals("NVARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.VARCHAR;
            }
            else if (str.Equals("DECIMAL", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.DECIMAL;
            }
            else if (str.Equals("BOOLEAN", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.BOOLEAN;
            }
            else if (str.Equals("DATETIME", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.DATETIME;
            }
            else
            {
                operandType = ExpressionOperandType.INTEGER;
                return false;
            }

            return true;
        }

        internal abstract void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues);

        internal abstract void EvaluateContained(Stack<ExpressionOperand> stack);
    }
}

