namespace JankSQL
{
    internal class Filter : IComponentOutput
    {
        IComponentOutput myInput;
        List<Expression> predicateExpressionLists;

        internal Filter(IComponentOutput input, List<Expression> predicateExpressionLists)
        {
            this.Input = input;
            this.Predicates = predicateExpressionLists;
        }

        internal Filter()
        {
        }

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal List<Expression> Predicates { set { predicateExpressionLists = value; } }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);
            ResultSet rsOutput = ResultSet.NewWithShape(rsInput);

            //REVIEW: ignores max
            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // evaluate the where clauses, if any
                bool predicatePassed = true;
                foreach (var p in predicateExpressionLists)
                {
                    ExpressionOperand result = p.Evaluate(new RowsetValueAccessor(rsInput, i));

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