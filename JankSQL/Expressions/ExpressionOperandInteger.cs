namespace JankSQL
{
    internal class ExpressionOperandInteger : ExpressionOperand, IComparable<ExpressionOperandInteger>, IEquatable<ExpressionOperandInteger>
    {
        private int n;

        internal ExpressionOperandInteger(int n)
            : base(ExpressionOperandType.INTEGER)
        {
            this.n = n;
        }

        public override object Clone()
        {
            return new ExpressionOperandInteger(n);
        }

        public override string ToString()
        {
            return $"integer({n})";
        }

        public override double AsDouble()
        {
            return (double)n;
        }

        public override string AsString()
        {
            return $"{n}";
        }

        public override int AsInteger()
        {
            return n;
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return other.AsDouble() == AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.NVARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return other.AsDouble() == AsDouble();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() > other.AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDouble() > other.AsDouble();
            }
            else
            {
                throw new NotImplementedException("INTEGER GreaterThan");
            }
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() < other.AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDouble() < other.AsDouble();
            }
            else
            {
                throw new NotImplementedException("INTEGER LessThan");
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
                throw new InvalidOperationException("OperatorPlus Integer");
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
                throw new InvalidOperationException("OperatorMinus Integer");
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
                throw new InvalidOperationException("OperatorSlash Integer");
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
                throw new InvalidOperationException("OperatorTimes Integer");
            }
        }

        public override void AddToSelf(ExpressionOperand other)
        {
            n += other.AsInteger();
        }

        public int CompareTo(ExpressionOperandInteger? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            int result = n.CompareTo(other.n);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            ExpressionOperandInteger o = (ExpressionOperandInteger)other;
            int result = n.CompareTo(o.n);
            return result;
        }

        public bool Equals(ExpressionOperandInteger? other)
        {
            return 0 == CompareTo(other);
        }

        public override bool Equals(object? o)
        {
            ExpressionOperandInteger? other = o as ExpressionOperandInteger;
            if (other == null)
                return false;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return n.GetHashCode();
        }
    }
}

