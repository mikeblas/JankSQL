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

        public ResultSet GetRows(int max)
        {
            if (outputExhausted)
            {
                ResultSet endSet = ResultSet.NewWithShape(totalResults);
                endSet.MarkEOF();
                return endSet;
            }

            while (!inputExhausted)
            {
                ResultSet rs = myInput.GetRows(5);
                if (rs.IsEOF)
                {
                    inputExhausted = true;
                    break;
                }

                if (totalResults == null)
                    totalResults = ResultSet.NewWithShape(rs);

                totalResults.Append(rs);
            }

            // if totalResults is null at this point, it means we had no input at all.
            if (totalResults != null)
            {
                //TODO: honor max
                // we've completely built totalResults, so sort it
                var evaluatingComparer = new EvaluatingComparer(sortExpressions, isAscending, totalResults.GetColumnNames());
                totalResults.Sort(evaluatingComparer);
                Console.WriteLine($"Sorted! {evaluatingComparer.KeyComparisons} key comparisons, {evaluatingComparer.RowComparisons} row comparisons");
            }

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
