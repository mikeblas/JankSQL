namespace JankSQL.Expressions
{
    internal class ExpressionOperandBoolean : ExpressionOperand
    {
        private readonly bool b;

        internal ExpressionOperandBoolean(bool b)
            : base(ExpressionOperandType.BOOLEAN)
        {
            this.b = b;
        }

        public override object Clone()
        {
            return new ExpressionOperandBoolean(b);
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

        public override int AsInteger()
        {
            throw new NotImplementedException();
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

        public override void AddToSelf(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ExpressionOperandBoolean? other)
        {
            if (other == null)
                throw new ArgumentNullException("obj");

            int result = b.CompareTo(other.b);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            ExpressionOperandBoolean o = (ExpressionOperandBoolean)other;
            int result = b.CompareTo(o.b);
            return result;
        }

        public bool Equals(ExpressionOperandDecimal? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            ExpressionOperandDecimal? o = obj as ExpressionOperandDecimal;
            if (o == null)
                return false;
            return Equals(o);
        }

        public override int GetHashCode()
        {
            return b.GetHashCode();
        }
    }
}

