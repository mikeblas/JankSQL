namespace JankSQL.Operators
{
    internal class Filter : IComponentOutput
    {
        IComponentOutput myInput;
        List<Expression> predicateExpressionLists;

        internal Filter(IComponentOutput input, List<Expression> predicateExpressionLists)
        {
            myInput = input;
            this.predicateExpressionLists = predicateExpressionLists;
        }

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal List<Expression> Predicates { set { predicateExpressionLists = value; } }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet? rsInput = myInput.GetRows(max);
            if (rsInput == null)
                return null;
            ResultSet rsOutput = ResultSet.NewWithShape(rsInput);

            //TODO: ignores max
            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // evaluate the where clauses, if any
                bool predicatePassed = true;
                foreach (var p in predicateExpressionLists)
                {
                    ExpressionOperand result = p.Evaluate(new ResultSetValueAccessor(rsInput, i));

                    if (!result.IsTrue())
                    {
                        predicatePassed = false;
                        break;
                    }
                }

                if (!predicatePassed)
                    continue;

                rsOutput.AddRowFrom(rsInput, i);
            }

            return rsOutput;
        }
    }
}