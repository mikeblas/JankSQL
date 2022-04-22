namespace JankSQL.Expressions
{
    public class ExpressionOperandBookmark : ExpressionOperand
    {
        private readonly Tuple tuple;

        internal ExpressionOperandBookmark(Tuple tuple)
            : base(ExpressionOperandType.BOOKMARK)
        {
            this.tuple = tuple;
        }

        public override bool RepresentsNull => false;

        public Tuple Tuple => tuple;

        public override object Clone()
        {
            return new ExpressionOperandBookmark(tuple);
        }

        public override string ToString()
        {
            return $"Bookmark({tuple})";
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
            if (other == null)
                throw new ArgumentNullException("obj");
            if (other.tuple.Length != tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {tuple.Length} other is {other.tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < tuple.Length)
            {
                ret = tuple[index].CompareTo(other.tuple[index]);
            }

            return ret;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            ExpressionOperandBookmark o = (ExpressionOperandBookmark)other;
            if (o.tuple.Length != tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {tuple.Length} other is {o.tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < tuple.Length)
            {
                ret = tuple[index].CompareTo(o.tuple[index]);
            }

            return ret;
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
            ts.WriteTo(tuple, stream);
        }

        internal static ExpressionOperandBookmark FromByteStream(Stream stream)
        {
            var ts = new Engines.TupleSerializer();
            Tuple t = ts.ReadFrom(stream);

            return new ExpressionOperandBookmark(t);
        }
    }
}

