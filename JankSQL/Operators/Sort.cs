namespace JankSQL.Operators
{
    internal class EvaluatingComparer : IComparer<ExpressionOperand[]>
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

        public int Compare(ExpressionOperand[]? x, ExpressionOperand[]? y)
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

    internal class Sort : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly bool[] isAscending;
        private readonly Expression[] sortExpressions;

        private ResultSet? totalResults;
        private bool inputExhausted = false;
        private bool outputExhausted = false;

        public Sort(IComponentOutput myInput, List<Expression> sortKeyList, List<bool> isAscendingList)
        {
            this.myInput = myInput;
            sortExpressions = sortKeyList.ToArray();
            isAscending = isAscendingList.ToArray();
        }

        public ResultSet? GetRows(int max)
        {
            if (outputExhausted)
                return null;

            while (!inputExhausted)
            {
                ResultSet? rs = myInput.GetRows(5);
                if (rs == null)
                {
                    inputExhausted = true;
                    break;
                }

                if (totalResults == null)
                    totalResults = ResultSet.NewWithShape(rs);
                totalResults.Append(rs);
            }

            //TODO: honor max
            // we've completely built totalResults, so sort it
            var evaluatingComparer = new EvaluatingComparer(sortExpressions, isAscending, totalResults.GetColumnNames());
            totalResults.Sort(evaluatingComparer);
            Console.WriteLine($"Sorted! {evaluatingComparer.KeyComparisons} key comparisons, {evaluatingComparer.RowComparisons} row comparisons");

            // and send it off
            outputExhausted = true;
            return totalResults;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
