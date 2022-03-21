namespace JankSQL
{
    public enum ExpressionOperandType
    {
        BOOKMARK,
        VARCHAR,
        NVARCHAR,
        INTEGER,
        DECIMAL,
        BOOLEAN,
    }

    public class ExpressionNode
    {

        public static bool TypeFromString(string str, out ExpressionOperandType operandType)
        {
            if (str.Equals("INTEGER", StringComparison.OrdinalIgnoreCase) || str.Equals("INT", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.INTEGER;
            }
            else if (str.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.VARCHAR;
            }
            else if (str.Equals("NVARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.NVARCHAR;
            }
            else if (str.Equals("DECIMAL", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.DECIMAL;
            }
            else if (str.Equals("BOOLEAN", StringComparison.OrdinalIgnoreCase))
            {
                operandType = ExpressionOperandType.BOOLEAN;
            }
            else
            {
                operandType = ExpressionOperandType.INTEGER;
                return false;
            }

            return true;
        }
    }
}

