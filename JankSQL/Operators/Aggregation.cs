
namespace JankSQL
{

    interface IAggregateAccumulator
    {
        void Accumulate(ExpressionOperand op);
        ExpressionOperand FinalValue();
    }

    class SumAccumulator : IAggregateAccumulator
    {
        ExpressionOperand? sum;

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

    class MaxAccumulator : IAggregateAccumulator
    {
        ExpressionOperand? max;

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


    class MinAccumulator : IAggregateAccumulator
    {
        ExpressionOperand? min;

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


    class CountAccumulator : IAggregateAccumulator
    {
        int count = 0;

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


    class AvgAccumulator : IAggregateAccumulator
    {
        int count = 0;
        ExpressionOperand? sum;

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
        readonly List<AggregationOperatorType> operatorTypes;
        readonly List<Expression> expressions;
        readonly List<string> expressionNames;
        readonly List<Expression>? groupByExpressions;
        readonly Dictionary<ExpressionOperand[], List<IAggregateAccumulator>> dictKeyToAggs;
        readonly List<string>? groupByExpressionBindNames;

        readonly List<FullColumnName> outputNames = new();

        IComponentOutput myInput;
        bool inputExhausted;
        bool outputExhausted;


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

        List<IAggregateAccumulator> GetAccumulatorRow()
        {
            List<IAggregateAccumulator> accumulators = new();

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

            foreach(var kv in dictKeyToAggs)
            {
                Console.Write($"Aggregated key: {String.Join(",", kv.Key.Select(x => "[" + x + "]"))}: ");
                Console.Write($"{String.Join(",", kv.Value.Select(x => "[" + x.FinalValue() + "]"))}");
                Console.WriteLine();
            }

            ResultSet resultSet = new();
            resultSet.SetColumnNames(outputNames);

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

        void BuildOutputColumnNames()
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

        void ReadInput()
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
                    var accessor = new RowsetValueAccessor(rs, i);
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

        ExpressionOperand[] EvaluateGroupByKey(RowsetValueAccessor accessor)
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

