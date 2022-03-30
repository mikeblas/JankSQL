﻿namespace JankSQL
{
    internal class ExpressionOperandNVARCHAR : ExpressionOperand, IComparable<ExpressionOperandNVARCHAR>, IEquatable<ExpressionOperandNVARCHAR>
    {
        private string str;

        internal ExpressionOperandNVARCHAR(string str)
            : base(ExpressionOperandType.NVARCHAR)
        {
            this.str = str;
        }

        public override object Clone()
        {
            return new ExpressionOperandNVARCHAR(str);
        }

        public override string ToString()
        {
            return $"NVARCHAR(\"{str}\")";
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

        public override void AddToSelf(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ExpressionOperandNVARCHAR? other)
        {
            if (other == null)
                throw new ArgumentNullException("obj");

            int result = str.CompareTo(other.str);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            ExpressionOperandNVARCHAR o = (ExpressionOperandNVARCHAR)other;
            int result = str.CompareTo(o.str);
            return result;
        }

        public bool Equals(ExpressionOperandNVARCHAR? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            ExpressionOperandNVARCHAR? o = obj as ExpressionOperandNVARCHAR;
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

