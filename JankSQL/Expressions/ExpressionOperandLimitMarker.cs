namespace JankSQL.Expressions
{
    internal class ExpressionOperandLimitMarker : ExpressionOperand, IComparable<ExpressionOperand>, IEquatable<ExpressionOperand>
    {
        private readonly LimitMarkerType markerType;

        internal ExpressionOperandLimitMarker(LimitMarkerType markerType)
            : base(ExpressionOperandType.LIMITMARKER)
        {
            this.markerType = markerType;
        }


        public override bool RepresentsNull
        {
            get { return false; }
        }

        public override object Clone()
        {
            return new ExpressionOperandLimitMarker(markerType);
        }

        public override string ToString()
        {
            return $"LimitMarker({markerType})";
        }

        public override double AsDouble()
        {
            throw new InvalidOperationException("can't convert a limit marker");
        }

        public override string AsString()
        {
            throw new InvalidOperationException("can't convert a limit marker");
        }

        public override int AsInteger()
        {
            throw new InvalidOperationException("can't convert a limit marker");
        }

        public override DateTime AsDateTime()
        {
            throw new InvalidOperationException("can't convert a limit marker");
        }

        public override bool IsTrue()
        {
            throw new InvalidOperationException("can't convert a limit marker");
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't use comparison operators on a limit marker");
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't use comparison operators on a limit marker");
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't use comparison operators on a limit marker");
        }


        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }


        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorModulo(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }


        public override void AddToSelf(ExpressionOperand other)
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorUnaryMinus()
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorUnaryPlus()
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override ExpressionOperand OperatorUnaryTilde()
        {
            throw new InvalidOperationException("can't math on a limit marker");
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (other.NodeType == ExpressionOperandType.LIMITMARKER)
            {
                ExpressionOperandLimitMarker o = (ExpressionOperandLimitMarker)other;
                if (markerType == LimitMarkerType.LOWEST_POSSIBLE && o.markerType == LimitMarkerType.HIGHEST_POSSIBLE)
                    return -1;
                if (markerType == LimitMarkerType.HIGHEST_POSSIBLE && o.markerType == LimitMarkerType.LOWEST_POSSIBLE)
                    return 1;

                return 0;
            }

            if (markerType == LimitMarkerType.LOWEST_POSSIBLE)
                return -1;
            else
                return 1;
        }


        public bool Equals(ExpressionOperand? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? o)
        {
            if (o is not ExpressionOperandInteger other)
                return false;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return markerType.GetHashCode();
        }

        internal static ExpressionOperand FromByteStream(Stream stream)
        {
            throw new InvalidOperationException("Limit markers can not be serialized");
        }

        internal override void WriteToByteStream(Stream stream)
        {
            throw new InvalidOperationException("Limit markers can not be serialized");
        }
    }
}

