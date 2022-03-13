namespace JankSQL
{
    public abstract class ExpressionOperand : ExpressionNode, ICloneable
    {
        internal ExpressionOperandType nodeType;

        internal static ExpressionOperand DecimalFromString(bool isNegative, string str)
        {
            double d = Double.Parse(str);
            if (isNegative)
                d *= -1;
            return new ExpressionOperandDecimal(d);
        }

        internal static ExpressionOperand IntegerFromString(bool isNegative, string str)
        {
            int n = Int32.Parse(str);
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
            Console.WriteLine($"{o.GetType()}");

            if (o.GetType() == typeof(Int32))
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
                else if (opType == ExpressionOperandType.NVARCHAR)
                    return NVARCHARFromString((string)o);
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

        public static ExpressionOperandType IntegerOrDecimal(string str)
        {
            if (str.IndexOf('.') != -1)
                return ExpressionOperandType.DECIMAL;
            else
                return ExpressionOperandType.INTEGER;
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

        internal ExpressionOperand(ExpressionOperandType t)
        {
            nodeType = t;
        }

        public abstract bool IsTrue();
        public abstract string AsString();

        public abstract double AsDouble();

        public abstract int AsInteger();

        public ExpressionOperandType NodeType { get { return nodeType; } }

        public abstract bool OperatorEquals(ExpressionOperand other);
        public abstract bool OperatorGreaterThan(ExpressionOperand other);
        public abstract bool OperatorLessThan(ExpressionOperand other);
        public abstract ExpressionOperand OperatorPlus(ExpressionOperand other);
        public abstract ExpressionOperand OperatorMinus(ExpressionOperand other);
        public abstract ExpressionOperand OperatorTimes(ExpressionOperand other);
        public abstract ExpressionOperand OperatorSlash(ExpressionOperand other);

        public abstract object Clone();
    }
}

