using System.Diagnostics;

namespace JankSQL.Expressions
{
    public class ExpressionOperandBookmark : ExpressionOperand
    {
        internal ExpressionOperandBookmark(Tuple tuple)
            : base(ExpressionOperandType.BOOKMARK)
        {
            this.Tuple = tuple;
        }

        public override bool RepresentsNull => false;

        public Tuple Tuple { get; }

        public override object Clone()
        {
            return new ExpressionOperandBookmark(Tuple);
        }

        public override string ToString()
        {
            return $"Bookmark({Tuple})";
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
            throw new NotImplementedException();
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

        public int CompareTo(ExpressionOperandBookmark? other)
        {
            Debug.Assert(other != null, "Don't expect to compare null ExpressionOperandBookmarks");
            if (other.Tuple.Length != Tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {Tuple.Length} other is {other.Tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < Tuple.Length)
            {
                ret = Tuple[index].CompareTo(other.Tuple[index]);
                index++;
            }

            return ret;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            Debug.Assert(other != null, "Don't expect to compare null ExpressionOPerands");

            ExpressionOperandBookmark o = (ExpressionOperandBookmark)other;
            if (o.Tuple.Length != Tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {Tuple.Length} other is {o.Tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < Tuple.Length)
            {
                ret = Tuple[index].CompareTo(o.Tuple[index]);
                index++;
            }

            return ret;
        }

        internal static ExpressionOperandBookmark FromByteStream(Stream stream)
        {
            var ts = new Engines.TupleSerializer();
            Tuple t = ts.ReadFrom(stream);

            return new ExpressionOperandBookmark(t);
        }

        internal static ExpressionOperandBookmark FromInteger(int mark)
        {
            var ret = new ExpressionOperandBookmark(Tuple.FromSingleValue(mark));
            return ret;
        }

        internal override void WriteToByteStream(Stream stream)
        {
            WriteTypeAndNullness(stream);

            var ts = new Engines.TupleSerializer();
            ts.WriteTo(Tuple, stream);
        }
    }
}
