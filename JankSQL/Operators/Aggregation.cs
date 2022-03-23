namespace JankSQL.Operators
{
    using JankSQL.Contexts;
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
        public ResultSet? GetRows(int max)
        {
            if (outputExhausted)
                return null;

            if (!inputExhausted)
            {
                ReadInput();
                BuildOutputColumnNames();
            }

            foreach (var kv in dictKeyToAggs)
                Console.WriteLine($"Aggregated key: {kv.Key}: {kv.Value}");

            ResultSet resultSet = new (outputNames);

            if (groupByExpressions == null || groupByExpressions.Count == 0)
            {
                // with no group by, we should have exactly one row with
                // the number of columns equal to the number of aggregaton expressions
                Tuple outputRow = Tuple.CreateEmpty(expressions.Count);

                var kvFirst = dictKeyToAggs.First();
                for (int j = 0; j < kvFirst.Value.Count; j++)
                {
                    Console.WriteLine($"Expression: {expressions[j]}");
                    outputRow[j] = kvFirst.Value[j].FinalValue();
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
            throw new NotImplementedException();
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

        protected void ReadInput()
        {
            while (!inputExhausted)
            {
                ResultSet? rs = myInput.GetRows(5);
                if (rs == null)
                {
                    inputExhausted = true;
                    break;
                }

                // for each row, work over each aggregation
                for (int i = 0; i < rs.RowCount; i++)
                {
                    // first, evaluate groupByExpressions
                    var accessor = new ResultSetValueAccessor(rs, i);
                    Tuple groupByKey = EvaluateGroupByKey(accessor);

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
                        ExpressionOperand result = expressions[j].Evaluate(accessor);
                        aggs[j].Accumulate(result);
                    }
                }
            }
        }

        protected Tuple EvaluateGroupByKey(ResultSetValueAccessor accessor)
        {
            // this key is used as an identity for non-grouped expressions
            if (groupByExpressions == null || groupByExpressions.Count == 0)
                return Tuple.FromSingleValue(1);

            Tuple result = Tuple.CreateEmpty(groupByExpressions.Count);
            for (int i = 0; i < groupByExpressions.Count; i++)
                result[i] = groupByExpressions[i].Evaluate(accessor);

            return result;
        }

    }
}

