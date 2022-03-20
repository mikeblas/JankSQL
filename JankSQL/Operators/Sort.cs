
namespace JankSQL
{
    internal class Sort : IComponentOutput
    {
        IComponentOutput myInput;
        List<Expression> sortKeyList;
        ResultSet totalResults;
        bool inputExhausted = false;
        bool outputExhausted = false;

        public Sort(IComponentOutput myInput, List<Expression> sortKeyList)
        {
            this.myInput = myInput;
            this.sortKeyList = sortKeyList;
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


            outputExhausted = true;

            return totalResults;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
