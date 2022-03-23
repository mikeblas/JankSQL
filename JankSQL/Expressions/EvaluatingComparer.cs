namespace JankSQL
{
    internal class EvaluatingComparer : IComparer<Tuple>
    {
        private readonly Expression[] keyExpressions;
        private readonly bool[] isAscending;
        private readonly List<FullColumnName> columnNames;

        private int keyComparisons = 0;
        private int rowComparisons = 0;

        internal EvaluatingComparer(Expression[] keyExpressions, bool[] isAscending, List<FullColumnName> columnNames)
        {
            this.keyExpressions = keyExpressions;
            this.isAscending = isAscending;
            this.columnNames = columnNames;
        }

        internal int KeyComparisons
        {
            get { return keyComparisons; }
        }

        internal int RowComparisons
        {
            get { return rowComparisons; }
        }

        public int Compare(Tuple? x, Tuple? y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length)
                throw new ArgumentException($"sizes are different: {x.Length} and {y.Length}");

            var xAccessor = new TemporaryRowValueAccessor(x, columnNames);
            var yAccessor = new TemporaryRowValueAccessor(y, columnNames);

            int ret;
            int keyNumber = 0;
            do
            {
                ExpressionOperand xop = keyExpressions[keyNumber].Evaluate(xAccessor);
                ExpressionOperand yop = keyExpressions[keyNumber].Evaluate(yAccessor);
                ret = xop.CompareTo(yop);
                if (!isAscending[keyNumber])
                    ret = -ret;
                keyNumber++;
                keyComparisons += 1;
            }
            while (ret == 0 && keyNumber < keyExpressions.Length);

            rowComparisons += 1;
            return ret;
        }
    }
}
