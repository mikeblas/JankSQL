﻿namespace JankSQL
{
    internal class ExpressionOperandVARCHAR : ExpressionOperand, IComparable<ExpressionOperandVARCHAR>, IEquatable<ExpressionOperandVARCHAR>
    {
        private readonly string? str;
        private bool isNull;

        internal ExpressionOperandVARCHAR(string? str)
            : base(ExpressionOperandType.VARCHAR)
        {
            this.str = str;
            isNull = str == null;
        }

        internal ExpressionOperandVARCHAR(string? str, bool isNull)
            : base(ExpressionOperandType.VARCHAR)
        {
            this.str = str;
            if (this.str == null && !isNull)
                throw new InternalErrorException("got null value without null flag");
            if (this.str != null && isNull)
                throw new InternalErrorException("got null flag without null value");
            this.isNull = isNull;
        }

        public override bool RepresentsNull
        {
            get { return isNull; }
        }

        public override object Clone()
        {
            return new ExpressionOperandVARCHAR(str, isNull);
        }

        public override string ToString()
        {
            return $"VARCHAR(\"{(isNull ? "NULL" : str)}\")";
        }

        public override double AsDouble()
        {
            if (isNull || str == null)
                throw new InvalidOperationException("can't convert null NVARCHAR to double");
            return double.Parse(str);
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override string AsString()
        {
            if (isNull || str == null)
                throw new InvalidOperationException("can't convert null NVARCHAR to string");
            return str;
        }

        public override int AsInteger()
        {
            if (isNull || str == null)
                throw new InvalidOperationException("can't convert null NVARCHAR to integer");
            return int.Parse(str);
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.VARCHAR || other.NodeType == ExpressionOperandType.NVARCHAR)
            {
                return other.AsString() == AsString();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

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
            if (RepresentsNull || other.RepresentsNull)
                return false;

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandVARCHAR(null, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandVARCHAR(null, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandVARCHAR(null, true);

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
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandVARCHAR(null, true);

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

            if (isNull && other.isNull)
                return 0;
            if (isNull && !other.isNull)
                return -1;
            if (!isNull && other.isNull)
                return 1;

            int result = str!.CompareTo(other.str);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            ExpressionOperandVARCHAR o = (ExpressionOperandVARCHAR)other;
            return CompareTo(o);
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
            if (str == null)
                return 8675309;
            return str.GetHashCode();
        }
    }
}

