namespace JankSQL.Expressions
{
    internal class ExpressionOperandInteger : ExpressionOperand, IComparable<ExpressionOperandInteger>, IEquatable<ExpressionOperandInteger>
    {
        private int n;
        private bool isNull;

        internal ExpressionOperandInteger(int n)
            : base(ExpressionOperandType.INTEGER)
        {
            this.n = n;
            isNull = false;
        }


        internal ExpressionOperandInteger(int n, bool isNull)
            : base(ExpressionOperandType.INTEGER)
        {
            this.n = n;
            this.isNull = isNull;
        }

        public override bool RepresentsNull
        {
            get { return isNull; }
        }

        public override object Clone()
        {
            return new ExpressionOperandInteger(n, isNull);
        }

        public override string ToString()
        {
            return $"integer({(isNull ? "NULL" : n)})";
        }

        public override double AsDouble()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null INTEGER to double");
            return (double)n;
        }

        public override string AsString()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null INTEGER to string");
            return $"{n}";
        }

        public override int AsInteger()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null INTEGER to integer");
            return n;
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

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
            if (RepresentsNull || other.RepresentsNull)
                return false;

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
            if (RepresentsNull || other.RepresentsNull)
                return false;

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

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
            if (RepresentsNull)
                throw new InvalidOperationException("Can't increment NULL");
            n += other.AsInteger();
        }

        public override ExpressionOperand OperatorUnaryMinus()
        {
            if (RepresentsNull)
                return this;
            return new ExpressionOperandInteger(-n, false);
        }

        public override ExpressionOperand OperatorUnaryPlus()
        {
            if (RepresentsNull)
                return this;
            return new ExpressionOperandInteger(n, false);
        }

        public override ExpressionOperand OperatorUnaryTilde()
        {
            if (RepresentsNull)
                return this;
            return new ExpressionOperandInteger(~n, false);
        }

        public int CompareTo(ExpressionOperandInteger? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (isNull && other.isNull)
                return 0;
            if (isNull && !other.isNull)
                return -1;
            if (!isNull && other.isNull)
                return 1;

            int result = n.CompareTo(other.n);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            ExpressionOperandInteger o = (ExpressionOperandInteger)other;

            return CompareTo(o);
        }

        public bool Equals(ExpressionOperandInteger? other)
        {
            return CompareTo(other) == 0;
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
            if (isNull)
                return 8675309;
            return n.GetHashCode();
        }
    }
}

