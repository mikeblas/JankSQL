namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class GeneratedSeriesRowSource : IOperatorOutput
    {
        private readonly List<FullColumnName> columnNames;

        private readonly string? alias;

        private readonly Expression start;
        private ExpressionOperand? startValue;
        private readonly Expression end;
        private ExpressionOperand? endValue;
        private readonly Expression? step = null;
        private ExpressionOperand? stepValue = null;
        private int computedStepValue = 1;

        private bool descending = false;

        private int currentValue;

        private bool needsRewind;

        internal GeneratedSeriesRowSource(string? alias, Expression start, Expression end)
        {
            this.step = null;
            this.start = start;
            this.end = end;
            this.alias = alias;

            needsRewind = true;

            columnNames = new List<FullColumnName>();

            if (alias == null)
                columnNames.Add(FullColumnName.FromColumnName("value"));
            else
                columnNames.Add(FullColumnName.FromTableColumnName(alias, "value"));
        }

        internal GeneratedSeriesRowSource(string? alias, Expression start, Expression end, Expression step)
            : this(alias, start, end)
        {
            this.step = step;
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            return columnNames.ToArray();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumns, IDictionary<string, ExpressionOperand> bindValues)
        {
            return BindResult.Success();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (needsRewind)
            {
                startValue = start.Evaluate(outerAccessor, engine, bindValues);
                endValue = end.Evaluate(outerAccessor, engine, bindValues);

                // if step is not null, we do exactly what it says
                if (step != null)
                {
                    stepValue = step.Evaluate(outerAccessor, engine, bindValues);
                    if (stepValue.AsInteger() == 0)
                        throw new SemanticErrorException("step value of 0 is not acceptable");

                    if (stepValue.AsInteger() < 0)
                        descending = true;
                }
                else
                {
                    // no step, so we'll consider going backward if end is smaller than start
                    if (startValue.AsInteger() > endValue.AsInteger())
                    {
                        computedStepValue = -1;
                        descending = true;
                    }
                }

                currentValue = startValue.AsInteger();
                needsRewind = false;
            }

            if (startValue == null || endValue == null)
                throw new InternalErrorException("GeneratedSeriesRowSource was not rewound before producing rows");

            ResultSet resultSet = new (columnNames);

            if (!descending)
            {
                if (endValue.AsInteger() < currentValue)
                {
                    resultSet.MarkEOF();
                    return resultSet;
                }
            }
            else
            {
                if (endValue.AsInteger() > currentValue)
                {
                    resultSet.MarkEOF();
                    return resultSet;
                }
            }

            int t = 0;
            while (t < max && ((!descending && endValue.AsInteger() >= currentValue) || (descending && endValue.AsInteger() <= currentValue)))
            {
                //REVIEW: t isn't used, so this isn't paging correctly
                Tuple generatedValues = Tuple.CreateEmpty(columnNames.Count);

                generatedValues[0] = ExpressionOperand.IntegerFromInt(currentValue);

                resultSet.AddRow(generatedValues);

                // step by one if no step expression; otherwise use that expression
                if (stepValue != null)
                    currentValue += stepValue.AsInteger();
                else
                    currentValue += computedStepValue;

                t++;
            }

            return resultSet;
        }

        public void Rewind()
        {
            needsRewind = true;
        }
    }
}
