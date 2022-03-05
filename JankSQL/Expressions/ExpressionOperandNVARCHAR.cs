namespace JankSQL
{
    internal class ExpressionOperandNVARCHAR : ExpressionOperand
    {
        internal string str;
        internal ExpressionOperandNVARCHAR(string str)
            : base(ExpressionOperandType.NVARCHAR)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return $"NVARCHAR(\"{str}\")";
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

        public override int AsInteger()
        {
            return Int32.Parse(str);
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                return other.AsString() == AsString();
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return other.AsDouble() == AsDouble();
            }
            else
            {
                throw new InvalidOperationException();
            }
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
            ExpressionOperand ret;
            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                string result = AsString() + other.AsString();
                ret = new ExpressionOperandNVARCHAR(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double d = AsDouble() + other.AsDouble();
                ret = new ExpressionOperandDecimal(d);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return ret;
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

