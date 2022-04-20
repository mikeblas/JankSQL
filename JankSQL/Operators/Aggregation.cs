namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;
    using JankSQL.Operators.Aggregations;

    internal enum AggregationOperatorType
    {
        AVG,
        MAX,
        MIN,
        SUM,
        STDEV,
        STDEVP,
        VAR,
        VARP,
        COUNT,
        COUNT_BIG,
    }

    internal class Aggregation : IComponentOutput
    {
        private readonly List<AggregationOperatorType> operatorTypes;
        private readonly List<Expression> expressions;
        private readonly List<string> expressionNames;
        private readonly List<Expression>? groupByExpressions;
        private readonly Dictionary<Tuple, List<IAggregateAccumulator>> dictKeyToAggs;
        private readonly List<string>? groupByExpressionBindNames;

        private readonly List<FullColumnName> outputNames = new ();

        private readonly IComponentOutput myInput;
        private bool inputExhausted;
        private bool outputExhausted;

        internal Aggregation(IComponentOutput input, List<AggregateContext> contexts, List<Expression>? groupByExpressions, List<string>? groupByExpressionBindNames)
        {
            expressionNames = new List<string>();
            expressions = new List<Expression>();
            operatorTypes = new List<AggregationOperatorType>();
            dictKeyToAggs = new Dictionary<Tuple, List<IAggregateAccumulator>>(new ExpressionOperandEqualityComparator());
            this.groupByExpressionBindNames = groupByExpressionBindNames;

            foreach (var context in contexts)
            {
                if (context.ExpressionName == null)
                    throw new InternalErrorException($"Expected non-null expression name on {context.Expression}");

                expressions.Add(context.Expression);
                expressionNames.Add(context.ExpressionName);
                operatorTypes.Add(context.AggregationOperatorType);
            }

            this.groupByExpressions = groupByExpressions;
            myInput = input;
            inputExhausted = false;
            outputExhausted = false;
        }

        #region IComponentOutput implementation
        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (outputExhausted)
            {
                ResultSet endSet = new (outputNames);
                endSet.MarkEOF();
                return endSet;
            }

            if (!inputExhausted)
            {
                ReadInput(engine, outerAccessor, bindValues);
                BuildOutputColumnNames();
            }

            foreach (var kv in dictKeyToAggs)
                Console.WriteLine($"Aggregated key: {kv.Key}: [{string.Join(",", kv.Value)}]");

            ResultSet resultSet = new (outputNames);

            if (groupByExpressions == null || groupByExpressions.Count == 0)
            {
                // with no group by, we should have exactly one row with
                // the number of columns equal to the number of aggregaton expressions
                Tuple outputRow = Tuple.CreateEmpty(expressions.Count);

                if (dictKeyToAggs.Count == 0)
                {
                    // if the dictionary is empty, no row was presented so each aggregate
                    // will present its default value
                    List<IAggregateAccumulator> aggs = GetAccumulatorRow();

                    for (int i = 0; i < aggs.Count; i++)
                        outputRow[i] = aggs[i].FinalValue();
                }
                else
                {
                    var kvFirst = dictKeyToAggs.First();
                    for (int j = 0; j < kvFirst.Value.Count; j++)
                    {
                        Console.WriteLine($"Expression: {expressions[j]}");
                        outputRow[j] = kvFirst.Value[j].FinalValue();
                    }
                }

                resultSet.AddRow(outputRow);
                outputExhausted = true;
            }
            else
            {
                // with group bys, we'll hvae a row for each value that's in the keys to aggs dictionary
                foreach (var kv in dictKeyToAggs)
                {
                    // expressions.Count + groupByExpressions.Count
                    Tuple outputRow = Tuple.CreateEmpty(outputNames.Count);

                    int n = 0;

                    // values from this key (the GROUP BY keys)
                    if (groupByExpressionBindNames != null && groupByExpressionBindNames.Count > 0)
                    {
                        for (int j = 0; j < kv.Key.Length; j++)
                            outputRow[n++] = kv.Key[j];
                    }

                    // values from each aggregate expression
                    for (int j = 0; j < kv.Value.Count; j++)
                        outputRow[n++] = kv.Value[j].FinalValue();

                    resultSet.AddRow(outputRow);
                }

                //TODO: honor max
                outputExhausted = true;
            }

            return resultSet;
        }

        public void Rewind()
        {
            outputExhausted = false;
        }
        #endregion

        protected List<IAggregateAccumulator> GetAccumulatorRow()
        {
            List<IAggregateAccumulator> accumulators = new ();

            foreach (var operatorType in operatorTypes)
            {
                IAggregateAccumulator? accum = null;
                accum = operatorType switch
                {
                    AggregationOperatorType.SUM => new SumAccumulator(),
                    AggregationOperatorType.AVG => new AvgAccumulator(),
                    AggregationOperatorType.COUNT => new CountAccumulator(),
                    AggregationOperatorType.MIN => new MinAccumulator(),
                    AggregationOperatorType.MAX => new MaxAccumulator(),
                    _ => throw new NotImplementedException($"Can't yet accumulate {operatorType}"),
                };
                accumulators.Add(accum);
            }

            return accumulators;
        }


        protected void BuildOutputColumnNames()
        {
            int n = 0;
            if (groupByExpressionBindNames != null)
            {
                foreach (var x in groupByExpressionBindNames)
                {
                    outputNames.Add(FullColumnName.FromColumnName(x));
                    n++;
                }
            }

            foreach (var expressionName in expressionNames)
                outputNames.Add(FullColumnName.FromColumnName(expressionName));
        }

        protected void ReadInput(Engines.IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            while (!inputExhausted)
            {
                ResultSet rs = myInput.GetRows(engine, outerAccessor, 5, bindValues);
                if (rs.IsEOF)
                {
                    inputExhausted = true;
                    break;
                }

                // for each row, work over each aggregation
                for (int i = 0; i < rs.RowCount; i++)
                {
                    // first, evaluate groupByExpressions
                    var accessor = new CombinedValueAccessor(new ResultSetValueAccessor(rs, i), outerAccessor);
                    Tuple groupByKey = EvaluateGroupByKey(accessor, engine, bindValues);

                    // get a rack of accumulators for this key
                    List<IAggregateAccumulator> aggs;
                    if (!dictKeyToAggs.ContainsKey(groupByKey))
                    {
                        // new one!
                        aggs = GetAccumulatorRow();
                        dictKeyToAggs.Add(groupByKey, aggs);
                    }
                    else
                    {
                        aggs = dictKeyToAggs[groupByKey];
                    }

                    // evaluate each expression and offer it for them
                    for (int j = 0; j < expressions.Count; j++)
                    {
                        ExpressionOperand result = expressions[j].Evaluate(accessor, engine, bindValues);
                        aggs[j].Accumulate(result);
                    }
                }
            }
        }

        protected Tuple EvaluateGroupByKey(IRowValueAccessor accessor, Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            // this key is used as an identity for non-grouped expressions
            if (groupByExpressions == null || groupByExpressions.Count == 0)
                return Tuple.FromSingleValue(1);

            Tuple result = Tuple.CreateEmpty(groupByExpressions.Count);
            for (int i = 0; i < groupByExpressions.Count; i++)
            {
                try
                {
                    result[i] = groupByExpressions[i].Evaluate(accessor, engine, bindValues);
                }
                catch (ExecutionException ex)
                {
                    throw new ExecutionException($"Error evaluating GROUP BY expression {groupByExpressions[i]} due to: {ex.Message}");
                }
            }

            return result;
        }

    }
}

