
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
                sum = ExpressionOperand.FromObjectAndType(0, op.NodeType);

            if (sum.NodeType == ExpressionOperandType.DECIMAL)
                sum = ExpressionOperand.DecimalFromDouble(sum.AsDouble() + op.AsDouble());
            else if (sum.NodeType == ExpressionOperandType.INTEGER)
                sum = ExpressionOperand.IntegerFromInt(sum.AsInteger() + op.AsInteger());
            else
                throw new InvalidOperationException();
        }

        public ExpressionOperand FinalValue()
        {
            return sum;
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
            if (sum == null || count == 0)
                sum = ExpressionOperand.FromObjectAndType(0, op.NodeType);

            if (sum.NodeType == ExpressionOperandType.DECIMAL)
                sum = ExpressionOperand.DecimalFromDouble(sum.AsDouble() + op.AsDouble());
            else if (sum.NodeType == ExpressionOperandType.INTEGER)
                sum = ExpressionOperand.IntegerFromInt(sum.AsInteger() + op.AsInteger());
            else
                throw new InvalidOperationException();
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

        Dictionary<ExpressionOperand[], List<IAggregateAccumulator>> listDictResults2;

        readonly List<FullColumnName> outputNames = new();

        IComponentOutput myInput;
        bool inputExhausted;
        bool outputExhausted;


        internal Aggregation(IComponentOutput input, List<AggregateContext> contexts, List<Expression>? groupByExpressions)
        {
            expressionNames = new List<string>();
            expressions = new List<Expression>();
            operatorTypes = new List<AggregationOperatorType>();
            listDictResults2 = new Dictionary<ExpressionOperand[], List<IAggregateAccumulator>>(new ExpressionOperandEqualityComparator());

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
                switch (operatorType)
                {
                    case AggregationOperatorType.SUM:
                        accum = new SumAccumulator();
                        break;

                    case AggregationOperatorType.AVG:
                        accum = new AvgAccumulator();
                        break;

                    case AggregationOperatorType.COUNT:
                        accum = new CountAccumulator();

                        break;

                    default:
                        throw new NotImplementedException($"Can't yet accumulate {operatorType}");
                }
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

            foreach(var kv in listDictResults2)
            {
                Console.Write($"{String.Join(",", kv.Key.Select(x => "[" + x + "]"))}");

                foreach (var acc in kv.Value)
                {
                    Console.Write($"{acc.FinalValue()}, ");
                }

                Console.WriteLine();
            }

            ResultSet resultSet = new();
            resultSet.SetColumnNames(outputNames);

            if (groupByExpressions == null || groupByExpressions.Count == 0)
            {
                // with no group by, we should have exactly one row with
                // the number of columns equal to the number of aggregaton expressions
                ExpressionOperand[] outputRow = new ExpressionOperand[expressions.Count];

                var kvFirst = listDictResults2.First();
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
                throw new NotImplementedException();
            }


            return resultSet;
        }

        void BuildOutputColumnNames()
        {
            foreach (var expressionName in expressionNames)
            {
                outputNames.Add(FullColumnName.FromColumnName(expressionName));
            }
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
                    ExpressionOperand[] groupByKey = EvaluateGroupByKey(rs.Row(i));

                    // get a rack of accumulators for this key
                    List<IAggregateAccumulator> aggs;
                    if (!listDictResults2.ContainsKey(groupByKey))
                    {
                        // new one!
                        aggs = GetAccumulatorRow();
                        listDictResults2.Add(groupByKey, aggs);
                    }
                    else
                    {
                        aggs = listDictResults2[groupByKey];
                    }

                    // evaluate each expression and offer it for them
                    for (int j = 0; j < expressions.Count; j++)
                    {
                        ExpressionOperand result = expressions[j].Evaluate(new RowsetValueAccessor(rs, i));
                        aggs[j].Accumulate(result);
                    }
                }
            }
        }

        ExpressionOperand[] EvaluateGroupByKey(ExpressionOperand[] row)
        {
            if (groupByExpressions == null || groupByExpressions.Count == 0)
                return new ExpressionOperand[] { ExpressionOperand.IntegerFromInt(1) };

            throw new NotImplementedException("no group bys yet");
        }


        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}

