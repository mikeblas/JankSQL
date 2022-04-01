namespace JankSQL
{
    internal class ExpressionOperandDecimal : ExpressionOperand
    {
        private double d;
        private bool isNull;

        internal ExpressionOperandDecimal(double d)
            : base(ExpressionOperandType.DECIMAL)
        {
            this.d = d;
            isNull = false;
        }

        internal ExpressionOperandDecimal(double d, bool isNull)
            : base(ExpressionOperandType.DECIMAL)
        {
            this.d = d;
            this.isNull = isNull;
        }

        public override bool RepresentsNull
        {
            get { return isNull; }
        }


        public override object Clone()
        {
            return new ExpressionOperandDecimal(d, isNull);
        }


        public override string ToString()
        {
            return $"decimal({(isNull ? "NULL" : d)})";
        }

        public override double AsDouble()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DECIMAL to double");
            return d;
        }

        public override string AsString()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DECIMAL to string");
            return $"{d}";
        }

        public override int AsInteger()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DECIMAL to integer");
            return (int)d;
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
            else
            {
                throw new NotImplementedException("DECIMAL Equals");
            }
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() > other.AsDouble();
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
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.DECIMAL || other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsDouble() < other.AsDouble();
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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandDecimal(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandDecimal(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandDecimal(0, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandDecimal(0, true);

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
            if (RepresentsNull)
                throw new InvalidOperationException("Can't increment NULL");

            d += other.AsDouble();
        }

        public override ExpressionOperand OperatorUnaryMinus()
        {
            if (RepresentsNull)
                return this;
            return new ExpressionOperandDecimal(-d, false);
        }

        public override ExpressionOperand OperatorUnaryPlus()
        {
            if (RepresentsNull)
                return this;
            return new ExpressionOperandDecimal(-d, false);
        }

        public override ExpressionOperand OperatorUnaryTilde()
        {
            throw new NotImplementedException();
        }


        public int CompareTo(ExpressionOperandDecimal? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (isNull && other.isNull)
                return 0;
            if (isNull && !other.isNull)
                return -1;
            if (!isNull && other.isNull)
                return 1;

            int result = d.CompareTo(other.d);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            ExpressionOperandDecimal o = (ExpressionOperandDecimal)other;
            return d.CompareTo(o);
        }

        public bool Equals(ExpressionOperandDecimal? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            ExpressionOperandDecimal? o = obj as ExpressionOperandDecimal;
            if (o == null)
                return false;
            return Equals(o);
        }

        public override int GetHashCode()
        {
            if (isNull)
                return 8675309;
            return d.GetHashCode();
        }
    }
}

