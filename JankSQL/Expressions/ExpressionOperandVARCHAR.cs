namespace JankSQL
{
    internal class ExpressionOperandVARCHAR : ExpressionOperand
    {
        internal string str;
        internal ExpressionOperandVARCHAR(string str)
            : base(ExpressionOperandType.VARCHAR)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return $"VARCHAR(\"{str}\")";
        }

        public override double AsDouble()
        {
            return Double.Parse(str);
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override string AsString()
        {
            return str;
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                return other.AsString() == AsString();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) > 0;
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() > other.AsDouble();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) < 0;
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() < other.AsDouble();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            ExpressionOperand result;
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                string str = AsString() + other.AsString();
                result = new ExpressionOperandNVARCHAR(str);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double d = AsDouble() + other.AsDouble();
                result = new ExpressionOperandDecimal(d);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return result;
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                // can't subtract strings
                throw new InvalidOperationException("OperatorMinus string");
            }
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash string");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes string");
            }
        }
    }
}

