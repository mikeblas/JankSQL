namespace JankSQL
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    public abstract class ExpressionOperand : ExpressionNode, ICloneable, IComparable<ExpressionOperand>
    {
        internal ExpressionOperand(ExpressionOperandType t)
        {
            NodeType = t;
        }

        public ExpressionOperandType NodeType { get; }

        public abstract bool RepresentsNull { get; }

        public abstract bool IsTrue();

        public abstract string AsString();

        public abstract double AsDouble();

        public abstract int AsInteger();

        public abstract DateTime AsDateTime();

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

        public abstract int CompareTo(ExpressionOperand? other);

        internal static ExpressionOperand CreateFromByteStream(Stream stream)
        {
            ExpressionOperandType nodeType = (ExpressionOperandType)stream.ReadByte();

            int representsNull = stream.ReadByte();
            if (representsNull != 0)
                return ExpressionOperand.NullLiteral();

            ExpressionOperand ret = nodeType switch
            {
                ExpressionOperandType.BOOLEAN => ExpressionOperandBoolean.FromByteStream(stream),
                ExpressionOperandType.INTEGER => ExpressionOperandInteger.FromByteStream(stream),
                ExpressionOperandType.VARCHAR => ExpressionOperandVARCHAR.FromByteStream(stream),
                ExpressionOperandType.BOOKMARK => ExpressionOperandBookmark.FromByteStream(stream),
                ExpressionOperandType.DECIMAL => ExpressionOperandDecimal.FromByteStream(stream),
                ExpressionOperandType.DATETIME => ExpressionOperandDateTime.FromByteStream(stream),
                _ => throw new NotSupportedException($"unknown nodeType {nodeType}"),
            };
            return ret;
        }

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

        internal static ExpressionOperand DateTimeFromDateTime(DateTime dt)
        {
            return new ExpressionOperandDateTime(dt);
        }

        internal static ExpressionOperand IntegerFromInt(int n)
        {
            return new ExpressionOperandInteger(n);
        }

        internal static ExpressionOperand FromObjectAndType(object? o, ExpressionOperandType opType)
        {
            if (o == null)
                return NullLiteral();

            if (o is int oInt)
            {
                if (opType == ExpressionOperandType.INTEGER)
                    return IntegerFromInt(oInt);
                else if (opType == ExpressionOperandType.DECIMAL)
                    return DecimalFromDouble((double)oInt);
            }
            else if (o is string oString)
            {
                if (opType == ExpressionOperandType.VARCHAR)
                    return VARCHARFromString(oString);
                if (opType == ExpressionOperandType.DATETIME)
                {
                    if (DateTime.TryParse(oString, out DateTime dt))
                        return DateTimeFromDateTime(dt);
                }
            }
            else if (o is double oDouble)
            {
                if (opType == ExpressionOperandType.INTEGER)
                    return IntegerFromInt((int)oDouble);
                else if (opType == ExpressionOperandType.DECIMAL)
                    return DecimalFromDouble(oDouble);
            }

            throw new ArgumentException($"Can't make ExpressionOperand of {opType} out of {o.GetType()}");
        }

        internal static ExpressionOperandType IntegerOrDecimal(string str)
        {
            if (str.Contains('.'))
                return ExpressionOperandType.DECIMAL;
            else
                return ExpressionOperandType.INTEGER;
        }

        internal static ExpressionOperandType IntegerOrDecimal(ExpressionOperand op)
        {
            if (op.NodeType == ExpressionOperandType.DECIMAL || op.NodeType == ExpressionOperandType.INTEGER)
                return op.NodeType;

            if (op.AsString().Contains('.'))
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

        internal abstract void WriteToByteStream(Stream stream);

        internal void WriteTypeAndNullness(Stream stream)
        {
            stream.WriteByte((byte)NodeType);

            // describe our nullness
            if (RepresentsNull)
                stream.WriteByte(1);
            else
                stream.WriteByte(0);
        }

        internal override void Evaluate(IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            stack.Push(this);
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
