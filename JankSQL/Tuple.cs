namespace JankSQL
{
    using System.Collections;

    /// <summary>
    /// A tuple is a row: a set of expression values that are bundled together.
    /// </summary>
    public class Tuple : IEnumerable<ExpressionOperand>, IEnumerable
    {
        private ExpressionOperand[] values;

        internal Tuple()
        {
            values = Array.Empty<ExpressionOperand>();
        }

        protected Tuple(int columnCount)
        {
            values = new ExpressionOperand[columnCount];
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

        
        /// <summary>
        /// Access the expression value from the ith column
        /// </summary>
        /// <param name="i">column ordinal to retrieve</param>
        /// <returns>ExpressionOperand with that value</returns>
        public ExpressionOperand this[int i]
        {
            get { return values[i]; }
            set { values[i] = value; }
        }

        /// <summary>
        /// Add an expression value to the end of this tuple
        /// </summary>
        /// <param name="x">value to be added</param>
        public void Add(ExpressionOperand x)
        {
            ExpressionOperand[] n = new ExpressionOperand[values.Length + 1];
            Array.Copy(values, n, values.Length);
            n[values.Length] = x;
            values = n;
        }

        public IEnumerator<ExpressionOperand> GetEnumerator()
        {
            return values.Cast<ExpressionOperand>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", values.Select(x => $"[{x}]"));
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
            if (source.Length < count)
                throw new ArgumentException($"count {count} expected to be lower than length {source.Length}");
            var r = new Tuple(count);
            Array.Copy(source.values, 0, r.values, 0, count);
            return r;
        }

        internal static Tuple CreateSuperCopy(int count, Tuple source)
        {
            if (source.Length > count)
                throw new ArgumentException($"count {count} expected to be larger than length {source.Length}");
            var r = new Tuple(count);
            Array.Copy(source.values, 0, r.values, 0, source.Length);
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

            if (t == ExpressionOperandType.VARCHAR)
                tuple[0] = ExpressionOperand.VARCHARFromString(str);
            else
                throw new ArgumentException($"{nameof(t)} must be VARCHAR");

            return tuple;
        }

        internal static Tuple FromOperands(params ExpressionOperand[] ops)
        {
            var t = new Tuple(ops.Length);
            for (int i = 0; i < ops.Length; i++)
                t[i] = ops[i];
            return t;
        }
     }
}
