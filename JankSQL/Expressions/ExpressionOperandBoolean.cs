using System.Diagnostics;

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

        public override bool RepresentsNull
        {
            get { throw new NotImplementedException(); }
        }

        public override object Clone()
        {
            return new ExpressionOperandBoolean(b);
        }

        public override string ToString()
        {
            // return $"Boolean({(isNull ? "NULL" : b)})";
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

        public override DateTime AsDateTime()
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

        public override ExpressionOperand OperatorModulo(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override void AddToSelf(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorUnaryMinus()
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorUnaryPlus()
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorUnaryTilde()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ExpressionOperandBoolean? other)
        {
            Debug.Assert(other != null, "Don't expect to compare null ExpressionOperandBoolean objetcs");

            int result = b.CompareTo(other.b);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            Debug.Assert(other != null, "Don't expect to compare null ExpressionOperands");
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
            if (obj is not ExpressionOperandDecimal o)
                return false;
            return Equals(o);
        }

        public override int GetHashCode()
        {
            return b.GetHashCode();
        }

        internal static ExpressionOperandBoolean FromByteStream(Stream stream)
        {
            int bInt = stream.ReadByte();
            return new ExpressionOperandBoolean(bInt != 0);
        }

        internal override void WriteToByteStream(Stream stream)
        {
            WriteTypeAndNullness(stream);

            // then ourselves
            stream.WriteByte((byte)(b ? 1 : 0));
        }
    }
}