
namespace JankSQL
{
    internal class Sort : IComponentOutput
    {
        IComponentOutput myInput;
        List<Expression> sortKeyList;
        ResultSet totalResults;
        bool inputExhausted = false;
        bool outputExhausted = false;

        //TODO: hook up descending flags
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

            //TODO: needs our own IEXpressionOperandComparer that uses expressions instead of key ordinals
            // and understands descending
            /*
            int[] keyOrder = new int[sortKeyList.Count];
            keyOrder[0] = 1;

            Engines.IExpressionOperandComparer ic = new Engines.IExpressionOperandComparer(keyOrder);

            totalResults.Sort(ic);
            */

            outputExhausted = true;

            return totalResults;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
