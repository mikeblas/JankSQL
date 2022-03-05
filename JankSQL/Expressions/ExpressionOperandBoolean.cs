namespace JankSQL
{
    internal class ExpressionOperandBoolean : ExpressionOperand
    {
        internal bool b;
        internal ExpressionOperandBoolean(bool b)
            : base(ExpressionOperandType.NVARCHAR)
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
    }
}

