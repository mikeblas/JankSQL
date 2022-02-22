namespace JankSQL
{
    internal class Filter : IComponentOutput
    {
        IComponentOutput myInput;
        List<List<ExpressionNode>> predicateExpressionLists;

        internal Filter(IComponentOutput input, List<List<ExpressionNode>> predicateExpressionLists)
        {
            this.Input = input;
            this.Predicates = predicateExpressionLists;
        }

        internal Filter()
        {

        }

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal List<List<ExpressionNode>> Predicates { set { predicateExpressionLists = value; } }

        public ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);

            ResultSet rsOuptut = ResultSet.NewWithShape(rsInput);


            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // evaluate the where clauses, if any
                bool predicatePassed = true;
                foreach (var p in predicateExpressionLists)
                {
                    ExpressionOperand result = SelectListContext.Execute(p, rsInput, i);

                    if (!result.IsTrue())
                    {
                        predicatePassed = false;
                        break;
                    }
                }

                if (!predicatePassed)
                    continue;

                rsOuptut.AddRowFrom(rsInput, i);
            }


            return rsOuptut;
        }
    }
}