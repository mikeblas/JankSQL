namespace JankSQL.Expressions
{
    public abstract class ExpressionNode
    {
        public static bool TypeFromString(string str, out ExpressionOperandType operandType)
        {
            //TODO: convert to dictionary
            // types are always unaccented English, so we use OrdinalIgnoreCase
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

        internal abstract void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues);

        internal virtual BindResult Bind(Engines.IEngine engine, IList<FullColumnName> columns, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            // most expressions don't need to do bind, so this default implementation is always successful.
            return BindResult.Success();
        }
    }
}