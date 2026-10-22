namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Sort : IOperatorOutput
    {
        private readonly IOperatorOutput myInput;
        private readonly bool[] isAscending;
        private readonly Expression[] sortExpressions;

        private ResultSet? totalResults;
        private bool inputExhausted = false;
        private bool outputExhausted = false;

        public Sort(IOperatorOutput myInput, List<Expression> sortKeyList, List<bool> isAscendingList)
        {
            this.myInput = myInput;
            sortExpressions = sortKeyList.ToArray();
            isAscending = isAscendingList.ToArray();
        }
        public FullColumnName[] GetOutputColumnNames()
        {
            return myInput.GetOutputColumnNames();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            BindResult br = myInput.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;

            FullColumnName[] inputColumnNames = myInput.GetOutputColumnNames();

            foreach(var expr in sortExpressions)
            {
                br = expr.Bind(engine, inputColumnNames, outerColumnNames, bindValues);
                if (!br.IsSuccessful)
                    return br;
            }

            return BindResult.Success();
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (outputExhausted)
            {
                ResultSet endSet = ResultSet.NewWithShape(totalResults!);
                endSet.MarkEOF();
                return endSet;
            }

            while (!inputExhausted)
            {
                ResultSet rs = myInput.GetRows(engine, outerAccessor, 5, bindValues);
                if (rs.IsEOF)
                {
                    inputExhausted = true;
                    break;
                }

                if (totalResults == null)
                    totalResults = ResultSet.NewWithShape(rs);

                totalResults.Append(rs);
            }

            // if we had no input at all, we still have a totalResults because we had at least one zero-length ResultSet on our input
            // before seeing the IsEOF ResultSet object.
            if (totalResults == null)
                throw new InternalErrorException("didn't expect null ResultSet");

            //TODO: honor max
            // we've completely built totalResults, so sort it
            var evaluatingComparer = new EvaluatingComparer(engine, sortExpressions, isAscending, totalResults.GetColumnNames(), bindValues);
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
