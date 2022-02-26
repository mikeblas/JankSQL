using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public enum ExpressionNodeType
    {
        VARCHAR,
        NVARCHAR,
        INTEGER,
        DECIMAL,
    };

    public class ExpressionNode
    {
    }

    public class ExpressionOperator : ExpressionNode
    {
        internal string str;

        internal ExpressionOperator(string str)
        {
            this.str = str;
        }

        public override String ToString()
        {
            return str;
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            if (str == "/")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                ExpressionOperand result = left.OperatorSlash(right);
                return result;
            }
            else if (str == "+")
            {
                ExpressionOperand op1 = stack.Pop(); 
                ExpressionOperand op2 = stack.Pop();

                ExpressionOperand result = op2.OperatorPlus(op1);
                return result;
            }
            else if (str == "-")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                ExpressionOperand result = left.OperatorMinus(right);
                return result;
            }
            else if (str == "*")
            {
                ExpressionOperand op1 = stack.Pop();
                ExpressionOperand op2 = stack.Pop();

                ExpressionOperand result = op1.OperatorTimes(op2);
                return result;
            }
            else if (str == "SQRT")
            {
                ExpressionOperand op1 = stack.Pop();
                double d = Math.Sqrt(op1.AsDouble());
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "POWER")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                double d = Math.Pow(left.AsDouble(), right.AsDouble());
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }
        }
    }

    public abstract class ExpressionOperand : ExpressionNode
    {
        internal ExpressionNodeType nodeType;

        internal static ExpressionOperand DecimalFromString(bool isNegative, string str)
        {
            double d = Double.Parse(str);
            if (isNegative)
                d *= -1;
            return new ExpressionOperandDecmial(d);
        }

        internal static ExpressionOperand DecimalFromDouble(double d)
        {
            return new ExpressionOperandDecmial(d);
        }

        private static string NormalizeString(string str)
        {
            // remove 'N' if we have it
            string temp = str;
            if (str[0] == 'N')
                temp = str.Substring(1);

            // trim leading and trailing ticks
            temp = temp.Substring(1, temp.Length - 2);

            // unescape double ticks
            temp = temp.Replace("''", "'");
            return temp;
        }

        internal static ExpressionOperand NVARCHARFromString(string str)
        {
            return new ExpressionOperandNVARCHAR(str);
        }

        internal static ExpressionOperand NVARCHARFromStringContext(string str)
        {
            return new ExpressionOperandNVARCHAR(NormalizeString(str));
        }

        internal static ExpressionOperand VARCHARFromString(string str)
        {
            return new ExpressionOperandVARCHAR(str);
        }

        internal static ExpressionOperand VARCHARFromStringContext(string str)
        {
            return new ExpressionOperandVARCHAR(NormalizeString(str));
        }

        internal ExpressionOperand(ExpressionNodeType t)
        {
            nodeType = t;
        }

        public abstract bool IsTrue();
        public abstract string AsString();

        public abstract double AsDouble();

        public ExpressionNodeType NodeType { get { return nodeType; } }

        public abstract bool OperatorEquals(ExpressionOperand other);
        public abstract bool OperatorGreaterThan(ExpressionOperand other);
        public abstract bool OperatorLessThan(ExpressionOperand other);
        public abstract ExpressionOperand OperatorPlus(ExpressionOperand other);
        public abstract ExpressionOperand OperatorMinus(ExpressionOperand other);
        public abstract ExpressionOperand OperatorTimes(ExpressionOperand other);
        public abstract ExpressionOperand OperatorSlash(ExpressionOperand other);
    }

    internal class ExpressionOperandDecmial : ExpressionOperand
    {
        internal double d;
        internal ExpressionOperandDecmial(double d)
            : base(ExpressionNodeType.DECIMAL)
        {
            this.d = d;
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

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (other.AsDouble() == AsDouble());
            }
            else if (other.NodeType == ExpressionNodeType.NVARCHAR || other.NodeType == ExpressionNodeType.VARCHAR)
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
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (AsDouble() > other.AsDouble());
            }
            else if (other.NodeType == ExpressionNodeType.NVARCHAR || other.NodeType == ExpressionNodeType.VARCHAR)
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
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (AsDouble() < other.AsDouble());
            }
            else if (other.NodeType == ExpressionNodeType.NVARCHAR || other.NodeType == ExpressionNodeType.VARCHAR)
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
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorPlus Decimal");
            }
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorMinus Decimal");
            }
        }


        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash Decimal");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes Decimal");
            }
        }
    }


    internal class ExpressionOperandInteger : ExpressionOperand
    {
        internal int n;
        internal ExpressionOperandInteger(int n)
            : base(ExpressionNodeType.INTEGER)
        {
            this.n = n;
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

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (other.AsDouble() == AsDouble());
            }
            else if (other.NodeType == ExpressionNodeType.NVARCHAR || other.NodeType == ExpressionNodeType.VARCHAR)
            {
                return other.AsDouble() == AsDouble();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (AsDouble() > other.AsDouble());
            }
            else
            {
                throw new NotImplementedException("INTEGER GreaterThan");
            }
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                return (AsDouble() < other.AsDouble());
            }
            else
            {
                throw new NotImplementedException("INTEGER LessThan");
            }
        }


        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() + other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorPlus Integer");
            }
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorMinus Integer");
            }
        }


        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash Integer");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes Integer");
            }
        }
    }

    internal class ExpressionOperandNVARCHAR : ExpressionOperand
    {
        internal string str;
        internal ExpressionOperandNVARCHAR(string str)
            : base(ExpressionNodeType.NVARCHAR)
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

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return other.AsString() == AsString();
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) > 0;
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) < 0;
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                string result = AsString() + other.AsString();
                ret = new ExpressionOperandNVARCHAR(result);
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double d = AsDouble() + other.AsDouble();
                ret = new ExpressionOperandDecmial(d);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return ret;
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                // can't subtract strings
                throw new InvalidOperationException("OperatorMinus string");
            }
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash string");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes string");
            }
        }
    }

    internal class ExpressionOperandVARCHAR : ExpressionOperand
    {
        internal string str;
        internal ExpressionOperandVARCHAR(string str)
            : base(ExpressionNodeType.VARCHAR)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return other.AsString() == AsString();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) > 0;
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                return AsString().CompareTo(other.AsString()) < 0;
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
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
            if (other.NodeType == ExpressionNodeType.VARCHAR || other.NodeType == ExpressionNodeType.NVARCHAR)
            {
                string str = AsString() + other.AsString();
                result = new ExpressionOperandNVARCHAR(str);
            }
            else if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double d = AsDouble() + other.AsDouble();
                result = new ExpressionOperandDecmial(d);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return result;
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() - other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                // can't subtract strings
                throw new InvalidOperationException("OperatorMinus string");
            }
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorSlash string");
            }
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (other.NodeType == ExpressionNodeType.DECIMAL || other.NodeType == ExpressionNodeType.INTEGER)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecmial(result);
            }
            else
            {
                throw new InvalidOperationException("OperatorTimes string");
            }
        }
    }


    internal class ExpressionOperandBoolean : ExpressionOperand
    {
        internal bool b;
        internal ExpressionOperandBoolean(bool b)
            : base(ExpressionNodeType.NVARCHAR)
        {
            this.b = b;
        }

        public override string ToString()
        {
            return $"Boolean({b})";
        }

        public override double AsDouble()
        {
            throw new NotImplementedException();
        }

        public override string AsString()
        {
            throw new NotImplementedException();
        }

        public override bool IsTrue()
        {
            return b;
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }
        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }
    }

    internal class ExpressionOperandFromColumn : ExpressionNode
    {
        internal FullColumnName columnName;

        internal ExpressionOperandFromColumn(FullColumnName columnName)
        {
            this.columnName = columnName;
        }

        internal FullColumnName ColumnName { get { return columnName; } }

        public override string ToString()
        {
            return $"FromColumn({columnName})";
        }
    }


    internal class ExpressionComparisonOperator : ExpressionNode
    {
        internal string str;
        internal ExpressionComparisonOperator(string str)
        {
            this.str = str;
        }

        public override String ToString()
        {
            return str;
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result;

            if (str == ">")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorGreaterThan(right);
            }
            else if (str == "<")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorLessThan(right);
            }
            else if (str == "=")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorEquals(right);
            }
            else if (str == "<>" || str == "!=")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = !left.OperatorEquals(right);
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }

            return new ExpressionOperandBoolean(result);
        }
    }


    internal class ExpressionBooleanOperator : ExpressionNode
    {
        enum BooleanOperatorType
        {
            AND, OR, NOT
        }

        BooleanOperatorType opType;

        internal static ExpressionBooleanOperator GetOrOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.OR);
        }

        internal static ExpressionBooleanOperator GetAndOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.AND);
        }

        internal static ExpressionBooleanOperator GetNotOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.NOT);
        }

        ExpressionBooleanOperator(BooleanOperatorType opType)
        {
            this.opType = opType;
        }

        public override String ToString()
        {
            return opType.ToString();
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result = true;

            ExpressionOperand right = (ExpressionOperand)stack.Pop();

            switch (opType)
            {
                case BooleanOperatorType.AND:
                    {
                        ExpressionOperand left = (ExpressionOperand)stack.Pop();
                        result = right.IsTrue() && left.IsTrue();
                    }
                    break;

                case BooleanOperatorType.OR:
                    {
                        ExpressionOperand left = (ExpressionOperand)stack.Pop();
                        result = right.IsTrue() || left.IsTrue();
                    }
                    break;

                case BooleanOperatorType.NOT:
                    result = !right.IsTrue();
                    break;
            }

            return new ExpressionOperandBoolean(result);
        }
    }
}

