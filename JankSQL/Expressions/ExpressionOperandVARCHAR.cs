namespace JankSQL
{
    internal class ExpressionOperandVARCHAR : ExpressionOperand, IComparable<ExpressionOperandVARCHAR>, IEquatable<ExpressionOperandVARCHAR>
    {
        private readonly string str;

        internal ExpressionOperandVARCHAR(string str)
            : base(ExpressionOperandType.VARCHAR)
        {
            this.str = str;
        }

        public override object Clone()
        {
            return new ExpressionOperandVARCHAR(str);
        }

        public override string ToString()
        {
            return $"VARCHAR(\"{str}\")";
        }

        public override double AsDouble()
        {
            return double.Parse(str);
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
            return int.Parse(str);
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

        public override void AddToSelf(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ExpressionOperandVARCHAR? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            int result = str.CompareTo(other.str);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            ExpressionOperandVARCHAR o = (ExpressionOperandVARCHAR)other;
            int result = str.CompareTo(o.str);
            return result;
        }

        public bool Equals(ExpressionOperandVARCHAR? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            ExpressionOperandVARCHAR? o = obj as ExpressionOperandVARCHAR;
            if (o == null)
                return false;
            return Equals(o);
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }
    }
}

