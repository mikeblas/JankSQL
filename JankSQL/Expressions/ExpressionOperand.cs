namespace JankSQL
{
    using JankSQL.Expressions;

    public abstract class ExpressionOperand : ExpressionNode, ICloneable, IComparable<ExpressionOperand>
    {
        private readonly ExpressionOperandType nodeType;

        internal ExpressionOperand(ExpressionOperandType t)
        {
            nodeType = t;
        }

        public ExpressionOperandType NodeType
        {
            get { return nodeType; }
        }

        public abstract bool RepresentsNull { get; }

        public abstract bool IsTrue();

        public abstract string AsString();

        public abstract double AsDouble();

        public abstract int AsInteger();

        public abstract bool OperatorEquals(ExpressionOperand other);

        public abstract bool OperatorGreaterThan(ExpressionOperand other);

        public abstract bool OperatorLessThan(ExpressionOperand other);

        public abstract ExpressionOperand OperatorPlus(ExpressionOperand other);

        public abstract ExpressionOperand OperatorMinus(ExpressionOperand other);

        public abstract ExpressionOperand OperatorTimes(ExpressionOperand other);

        public abstract ExpressionOperand OperatorSlash(ExpressionOperand other);

        public abstract ExpressionOperand OperatorUnaryPlus();

        public abstract ExpressionOperand OperatorUnaryMinus();

        public abstract ExpressionOperand OperatorUnaryTilde();

        public abstract ExpressionOperand OperatorModulo(ExpressionOperand other);

        public abstract void AddToSelf(ExpressionOperand other);

        public abstract object Clone();

        public int Compare(ExpressionOperand? x, ExpressionOperand? y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            if (x.NodeType != y.NodeType)
                throw new ArgumentException($"can't compare {x.NodeType} to {y.NodeType}");

            int ret = x.Compare(x, y);
            return ret;
        }

        public abstract int CompareTo(ExpressionOperand? other);

        internal static ExpressionOperand DecimalFromString(bool isNegative, string str)
        {
            double d = double.Parse(str);
            if (isNegative)
                d *= -1;
            return new ExpressionOperandDecimal(d);
        }

        internal static ExpressionOperand NullLiteral()
        {
            return new ExpressionOperandInteger(0, true);
        }

        internal static ExpressionOperand IntegerFromString(bool isNegative, string str)
        {
            int n = int.Parse(str);
            if (isNegative)
                n *= -1;
            return new ExpressionOperandInteger(n);
        }

        internal static ExpressionOperand DecimalFromDouble(double d)
        {
            return new ExpressionOperandDecimal(d);
        }

        internal static ExpressionOperand IntegerFromInt(int n)
        {
            return new ExpressionOperandInteger(n);
        }

        internal static ExpressionOperand FromObjectAndType(object o, ExpressionOperandType opType)
        {
            if (o.GetType() == typeof(int))
            {
                if (opType == ExpressionOperandType.INTEGER)
                    return IntegerFromInt((int)o);
                else if (opType == ExpressionOperandType.DECIMAL)
                    return DecimalFromDouble((double)(int)o);
            }
            else if (o.GetType() == typeof(string))
            {
                if (opType == ExpressionOperandType.VARCHAR)
                    return VARCHARFromString((string)o);
            }
            else if (o.GetType() == typeof(double))
            {
                if (opType == ExpressionOperandType.INTEGER)
                    return DecimalFromDouble((double)(int)o);
                else if (opType == ExpressionOperandType.DECIMAL)
                    return DecimalFromDouble((double)o);
            }

            throw new ArgumentException($"Can't make ExpressionOperand of {opType} out of {o.GetType()}");
        }

        internal static ExpressionOperandType IntegerOrDecimal(string str)
        {
            if (str.IndexOf('.') != -1)
                return ExpressionOperandType.DECIMAL;
            else
                return ExpressionOperandType.INTEGER;
        }

        internal static ExpressionOperand VARCHARFromString(string str)
        {
            return new ExpressionOperandVARCHAR(str);
        }

        internal static ExpressionOperand VARCHARFromStringContext(string str)
        {
            return new ExpressionOperandVARCHAR(NormalizeString(str));
        }

        private static string NormalizeString(string str)
        {
            // remove 'N' if we have it
            string temp = str;
            if (str[0] == 'N')
                temp = str[1..];

            // trim leading and trailing ticks
            temp = temp[1..^1];

            // unescape double ticks
            temp = temp.Replace("''", "'");
            return temp;
        }
    }
}

