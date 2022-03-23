namespace JankSQL
{
    using System.Collections;

    public class Tuple : IEnumerable<ExpressionOperand>
    {
        private ExpressionOperand[] values;

        internal Tuple()
        {
            values = Array.Empty<ExpressionOperand>();
        }

        protected Tuple(int count)
        {
            values = new ExpressionOperand[count];
        }

        public ExpressionOperand[] Values
        {
            get { return values; }
        }

        public int Count
        {
            get { return values.Length; }
        }

        public int Length
        {
            get { return values.Length; }
        }

        public ExpressionOperand this[int i]
        {
            get { return values[i]; }
            set { values[i] = value; }
        }

        public void Add(ExpressionOperand x)
        {
            ExpressionOperand[] n = new ExpressionOperand[values.Length + 1];
            Array.Copy(values, n, values.Length);
            n[values.Length] = x;
            values = n;
        }

        internal static Tuple CreateEmpty(int count)
        {
            return new Tuple(count);
        }

        internal static Tuple CreateEmpty()
        {
            return new Tuple();
        }

        /// <summary>
        /// Creates a new Tuple of length count, then copies all the values from source
        /// into the first values of this new Tuple.  count must be >= source.Count.
        /// </summary>
        /// <param name="count">count of values in the new Tuple.</param>
        /// <param name="source">source Tuple to partially copy.</param>
        /// <returns>newly created Tuple.</returns>
        internal static Tuple CreatePartialCopy(int count, Tuple source)
        {
            var r = new Tuple(count);
            Array.Copy(source.values, 0, r.values, 0, source.Count);
            return r;
        }

        internal static Tuple CreateFromRange(Tuple source, int offset, int count)
        {
            var r = new Tuple(count);
            Array.Copy(source.values, offset, r.values, 0, count);
            return r;
        }

        internal static Tuple FromSingleValue(int n)
        {
            var tuple = new Tuple(1)
            {
                [0] = ExpressionOperand.IntegerFromInt(n),
            };
            return tuple;
        }

        internal static Tuple FromSingleValue(string str, ExpressionOperandType t)
        {
            var tuple = new Tuple(1);

            if (t == ExpressionOperandType.NVARCHAR)
                tuple[0] = ExpressionOperand.NVARCHARFromString(str);
            else if (t == ExpressionOperandType.VARCHAR)
                tuple[0] = ExpressionOperand.VARCHARFromString(str);
            else
                throw new ArgumentException($"{nameof(t)} must by NVARCHAR or VARCHAR");

            return tuple;
        }

        internal static Tuple FromOperands(params ExpressionOperand[] ops)
        {
            var t = new Tuple(ops.Length);
            for (int i = 0; i < ops.Length; i++)
                t[i] = ops[i];
            return t;
        }

        public IEnumerator<ExpressionOperand> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Join(",", values.Select(x => "[" + x + "]"));
        }
    }
}
