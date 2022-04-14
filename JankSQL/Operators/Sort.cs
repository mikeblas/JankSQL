namespace JankSQL.Operators
{
    using JankSQL.Expressions;

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

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max)
        {
            if (outputExhausted)
            {
                ResultSet endSet = ResultSet.NewWithShape(totalResults!);
                endSet.MarkEOF();
                return endSet;
            }

            while (!inputExhausted)
            {
                ResultSet rs = myInput.GetRows(engine, outerAccessor, 5);
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
                throw new InternalErrorException("didn't exepct null ResultSet");

            //TODO: honor max
            // we've completely built totalResults, so sort it
            var evaluatingComparer = new EvaluatingComparer(engine, sortExpressions, isAscending, totalResults.GetColumnNames());
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
