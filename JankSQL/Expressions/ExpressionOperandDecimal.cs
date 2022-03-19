namespace JankSQL
{
    internal class ExpressionOperandDecimal : ExpressionOperand
    {
        internal double d;
        internal ExpressionOperandDecimal(double d)
            : base(ExpressionOperandType.DECIMAL)
        {
            this.d = d;
        }

        public override object Clone()
        {
            return new ExpressionOperandDecimal(d);
        }


        public override string ToString()
        {
            return $"decimal({d})";
        }

        public override double AsDouble()
        {
            return d;
        }

        public override string AsString()
        {
            return $"{d}";
        }

        public override int AsInteger()
        {
            return (int)d;
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return (other.AsDouble() == AsDouble());
            }
            else if (other.NodeType == ExpressionOperandType.NVARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return other.AsDouble() == AsDouble();
            }
            else
            {
                throw new NotImplementedException("DECIMAL Equals");
            }
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return (AsDouble() > other.AsDouble());
            }
            else if (other.NodeType == ExpressionOperandType.NVARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDouble() > other.AsDouble();
            }
            else
            {
                throw new NotImplementedException("DECIMAL GreaterThan");
            }
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return (AsDouble() < other.AsDouble());
            }
            else if (other.NodeType == ExpressionOperandType.NVARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDouble() < other.AsDouble();
            }
            else
            {
                throw new NotImplementedException("DECIMAL LessThan");
            }
        }

        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorPlus Decimal");
            }
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorMinus Decimal");
            }
        }


        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash Decimal");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes Decimal");
            }
        }

        public override void AddToSelf(ExpressionOperand other)
        {
            d += other.AsDouble();
        }

        public int CompareTo(ExpressionOperandDecimal? other)
        {
            if (other == null)
                throw new ArgumentNullException("obj");

            int result = d.CompareTo(other.d);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            ExpressionOperandDecimal o = (ExpressionOperandDecimal)other;
            int result = d.CompareTo(o.d);
            return result;
        }

    }
}

