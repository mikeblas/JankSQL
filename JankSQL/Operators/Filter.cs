namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Filter : IOperatorOutput
    {
        private List<Expression> predicateExpressionLists;

        internal Filter(IOperatorOutput input, List<Expression> predicateExpressionLists)
        {
            Input = input;
            this.predicateExpressionLists = predicateExpressionLists;
        }

        internal IOperatorOutput Input { get; set; }

        internal List<Expression> Predicates
        {
            set { predicateExpressionLists = value; }
        }

        public void Rewind()
        {
            Input.Rewind();
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            return Input.GetOutputColumnNames();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            BindResult br = Input.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;

            FullColumnName[] inputColumnNames = Input.GetOutputColumnNames();

            foreach (var expr in predicateExpressionLists)
            {
                br = expr.Bind(engine, inputColumnNames, outerColumnNames, bindValues);
                if (!br.IsSuccessful)
                    return br;
            }

            return br;
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rsInput = Input.GetRows(engine, outerAccessor, max, bindValues);
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