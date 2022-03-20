
namespace JankSQL
{

    class EvaluatingComparer : IComparer<ExpressionOperand[]>
    {
        readonly Expression[] _keyExpressions;
        readonly bool[] _isAscending;
        readonly List<FullColumnName> _columnNames;

        int keyComparisons = 0;
        int rowComparisons = 0;


        internal EvaluatingComparer(Expression[] keyExpressions, bool[] isAscending, List<FullColumnName> columnNames)
        {
            _keyExpressions = keyExpressions;
            _isAscending = isAscending;
            _columnNames = columnNames;
        }

        internal int KeyComparisons { get { return keyComparisons; } }
        internal int RowComparisons { get { return rowComparisons; } }

        public int Compare(ExpressionOperand[]? x, ExpressionOperand[]? y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length)
                throw new ArgumentException($"sizes are different: {x.Length} and {y.Length}");

            var _xAccessor = new TemporaryRowValueAccessor(x, _columnNames);
            var _yAccessor = new TemporaryRowValueAccessor(y, _columnNames);

            int ret;
            int keyNumber = 0;
            do
            {
                ExpressionOperand xop = _keyExpressions[keyNumber].Evaluate(_xAccessor);
                ExpressionOperand yop = _keyExpressions[keyNumber].Evaluate(_yAccessor);
                ret = xop.CompareTo(yop);
                if (!_isAscending[keyNumber])
                    ret = -ret;
                keyNumber++;
                keyComparisons += 1;
            } while (ret == 0 && keyNumber < _keyExpressions.Length);

            rowComparisons += 1;
            return ret;
        }
    }


    internal class Sort : IComponentOutput
    {
        IComponentOutput myInput;
        bool[] isAscending;
        Expression[] sortExpressions;

        ResultSet? totalResults;
        bool inputExhausted = false;
        bool outputExhausted = false;

        
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
