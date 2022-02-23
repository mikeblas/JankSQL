using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal enum ExpressionNodeType
    {
        VARCHAR,
        NVARCHAR,
        INTEGER,
        DECIMAL,
    };

    internal class ExpressionNode
    {
    }

    internal class ExpressionOperator : ExpressionNode
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

        internal ExpressionOperand Evaluate(Stack<ExpressionNode> stack)
        {
            if (str == "/")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                double d = left.AsDouble() / right.AsDouble();
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "+")
            {
                ExpressionOperand op1 = (ExpressionOperand)stack.Pop();
                ExpressionOperand op2 = (ExpressionOperand)stack.Pop();

                double d = op1.AsDouble() + op2.AsDouble();
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "-")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                double d = left.AsDouble() - right.AsDouble();
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "*")
            {
                ExpressionOperand op1 = (ExpressionOperand)stack.Pop();
                ExpressionOperand op2 = (ExpressionOperand)stack.Pop();

                double d = op1.AsDouble() * op2.AsDouble();
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "SQRT")
            {
                ExpressionOperand op1 = (ExpressionOperand)stack.Pop();
                double d = Math.Sqrt(op1.AsDouble());
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "POWER")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

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

    internal abstract class ExpressionOperand : ExpressionNode
    {
        internal ExpressionNodeType nodeType;

        internal static ExpressionOperand DecimalFromString(string str)
        {
            return new ExpressionOperandDecmial(Double.Parse(str));
        }

        internal static ExpressionOperand DecimalFromDouble(double d)
        {
            return new ExpressionOperandDecmial(d);
        }

        internal static ExpressionOperand NVARCHARFromString(string str)
        {
            return new ExpressionOperandNVARCHAR(str);
        }

        internal ExpressionOperand(ExpressionNodeType t)
        {
            nodeType = t;
        }

        public abstract bool IsTrue();

        public abstract double AsDouble();
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

        public override bool IsTrue()
        {
            throw new NotImplementedException();
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
            return $"NVARCHAR({str})";
        }

        public override double AsDouble()
        {
            return Double.Parse(str);
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
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

        public override bool IsTrue()
        {
            return b;
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

        internal ExpressionOperand Evaluate(Stack<ExpressionNode> stack)
        {
            bool result;

            if (str == ">")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                if (left.AsDouble() > right.AsDouble())
                    result = true;
                else
                    result = false;
            }
            else if (str == "<")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                if (left.AsDouble() < right.AsDouble())
                    result = true;
                else
                    result = false;
            }
            else if (str == "=")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                if (left.AsDouble() == right.AsDouble())
                    result = true;
                else
                    result = false;
            }
            else if (str == "<>" || str == "!=")
            {
                ExpressionOperand right = (ExpressionOperand)stack.Pop();
                ExpressionOperand left = (ExpressionOperand)stack.Pop();

                if (left.AsDouble() != right.AsDouble())
                    result = true;
                else
                    result = false;
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

        internal ExpressionOperand Evaluate(Stack<ExpressionNode> stack)
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



