namespace JankSQL.Operators
{
    using JankSQL.Expressions;

    internal class Filter : IComponentOutput
    {
        private IComponentOutput myInput;
        private List<Expression> predicateExpressionLists;

        internal Filter(IComponentOutput input, List<Expression> predicateExpressionLists)
        {
            myInput = input;
            this.predicateExpressionLists = predicateExpressionLists;
        }

        internal IComponentOutput Input
        {
            get { return myInput; } set { myInput = value; }
        }

        internal List<Expression> Predicates
        {
            set { predicateExpressionLists = value; }
        }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rsInput = myInput.GetRows(engine, outerAccessor, max, bindValues);
            ResultSet rsOutput = ResultSet.NewWithShape(rsInput);

            if (rsInput.IsEOF)
            {
                rsOutput.MarkEOF();
                return rsOutput;
            }

            //TODO: ignores max
            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // evaluate the where clauses, if any
                bool predicatePassed = true;
                foreach (var p in predicateExpressionLists)
                {
                    ExpressionOperand result;

                    CombinedValueAccessor cva = new (new ResultSetValueAccessor(rsInput, i), outerAccessor);
                    result = p.Evaluate(cva, engine, bindValues);

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