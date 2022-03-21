namespace JankSQL.Operators
{
    using JankSQL.Contexts;

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

    /// <summary>
    /// Interface for an accumulator object used by the aggregation operator. Is offered values to
    /// aggregate, and then asked for a FinalValue after the end of the rowset.
    /// </summary>
    internal interface IAggregateAccumulator
    {
        /// <summary>
        /// Accepts a value to accumlate in an aggregation. Called once per row within a grouping.
        /// </summary>
        /// <param name="op">ExpressionOperand with the value to aggregate.</param>
        void Accumulate(ExpressionOperand op);

        /// <summary>
        /// Produces the final value of the aggregation. Called only once per grouping.
        /// </summary>
        /// <returns>an ExpressionOperand representing the value of this aggregation.</returns>
        ExpressionOperand FinalValue();
    }

    internal class SumAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? sum;

        internal SumAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (sum == null)
                sum = op;
            else
                sum.AddToSelf(op);
        }

        public ExpressionOperand FinalValue()
        {
            return sum;
        }
    }

    internal class MaxAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? max;

        internal MaxAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (max == null)
                max = op;
            else
            {
                int comp = max.CompareTo(op);
                if (comp < 0)
                    max = op;
            }
        }

        public ExpressionOperand FinalValue()
        {
            return max;
        }
    }


    internal class MinAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? min;

        internal MinAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (min == null)
                min = op;
            else
            {
                int comp = min.CompareTo(op);
                if (comp > 0)
                    min = op;
            }
        }

        public ExpressionOperand FinalValue()
        {
            return min;
        }
    }

    internal class CountAccumulator : IAggregateAccumulator
    {
        private int count = 0;

        internal CountAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            count += 1;
        }

        public ExpressionOperand FinalValue()
        {
            return ExpressionOperand.IntegerFromInt(count);
        }
    }


    internal class AvgAccumulator : IAggregateAccumulator
    {
        private int count = 0;
        private ExpressionOperand? sum;

        internal AvgAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (sum == null)
                sum = op;
            else
                sum.AddToSelf(op);

            count += 1;
        }

        public ExpressionOperand FinalValue()
        {
            if (sum.NodeType == ExpressionOperandType.DECIMAL)
                return ExpressionOperand.DecimalFromDouble(sum.AsDouble() / count);
            else if (sum.NodeType == ExpressionOperandType.INTEGER)
                return ExpressionOperand.IntegerFromInt(sum.AsInteger() / count);
            else
                throw new InvalidOperationException();
        }
    }

    internal class Aggregation : IComponentOutput
    {
        private readonly List<AggregationOperatorType> operatorTypes;
        private readonly List<Expression> expressions;
        private readonly List<string> expressionNames;
        private readonly List<Expression>? groupByExpressions;
        private readonly Dictionary<ExpressionOperand[], List<IAggregateAccumulator>> dictKeyToAggs;
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
            dictKeyToAggs = new Dictionary<ExpressionOperand[], List<IAggregateAccumulator>>(new ExpressionOperandEqualityComparator());
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
            {
                Console.Write($"Aggregated key: {string.Join(",", kv.Key.Select(x => "[" + x + "]"))}: ");
                Console.Write($"{string.Join(",", kv.Value.Select(x => "[" + x.FinalValue() + "]"))}");
                Console.WriteLine();
            }

            ResultSet resultSet = new (outputNames);

            if (groupByExpressions == null || groupByExpressions.Count == 0)
            {
                // with no group by, we should have exactly one row with
                // the number of columns equal to the number of aggregaton expressions
                ExpressionOperand[] outputRow = new ExpressionOperand[expressions.Count];

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
                    ExpressionOperand[] outputRow = new ExpressionOperand[outputNames.Count];

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
                    ExpressionOperand[] groupByKey = EvaluateGroupByKey(accessor);

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

        protected ExpressionOperand[] EvaluateGroupByKey(ResultSetValueAccessor accessor)
        {
            if (groupByExpressions == null || groupByExpressions.Count == 0)
                return new ExpressionOperand[] { ExpressionOperand.IntegerFromInt(1) };

            ExpressionOperand[] result = new ExpressionOperand[groupByExpressions.Count];
            for (int i = 0; i < groupByExpressions.Count; i++)
                result[i] = groupByExpressions[i].Evaluate(accessor);

            return result;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}

