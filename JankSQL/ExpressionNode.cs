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
                ExpressionOperand right = (ExpressionOperand) stack.Pop();
                ExpressionOperand left = (ExpressionOperand) stack.Pop();

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
                ExpressionOperand op1 = (ExpressionOperand)stack.Pop();
                ExpressionOperand op2 = (ExpressionOperand)stack.Pop();

                double d = op1.AsDouble() - op2.AsDouble();
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

            return new ExpressionOperandDecmial(0.0);
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
    }



    internal class ExpressionOperandFromColumn : ExpressionNode
    {
        internal string columnName;

        internal ExpressionOperandFromColumn(string columnName)
        {
            this.columnName = columnName;
        }

        internal string ColumnName { get { return columnName; } }
    }
}



