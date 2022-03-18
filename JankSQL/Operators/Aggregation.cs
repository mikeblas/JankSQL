
namespace JankSQL
{
    internal class Aggregation : IComponentOutput
    {
        readonly List<AggregationOperatorType> operatorTypes;
        readonly List<Expression> expressions;
        readonly List<string> expressionNames;
        readonly List<Expression> groupByExpressions;

        List<Dictionary<ExpressionOperand[], ExpressionOperand>> listDictResults;

        readonly List<FullColumnName> outputNames = new();

        IComponentOutput myInput;
        bool inputExhausted;
        bool outputExhausted;


        internal Aggregation(IComponentOutput input, List<AggregateContext> contexts, List<Expression> groupByExpressions)
        {
            expressionNames = new List<string>();
            expressions = new List<Expression>();
            operatorTypes = new List<AggregationOperatorType>();
            listDictResults = new List<Dictionary<ExpressionOperand[], ExpressionOperand>>();

            foreach (var context in contexts)
            {
                expressions.Add(context.Expression);
                expressionNames.Add(context.ExpressionName);
                operatorTypes.Add(context.AggregationOperatorType);
                listDictResults.Add(new Dictionary<ExpressionOperand[], ExpressionOperand>(new ExpressionOperandEqualityComparator()));
            }

            this.groupByExpressions = groupByExpressions;
            myInput = input;
            inputExhausted = false;
            outputExhausted = false;
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


            for (int j = 0; j < expressions.Count; j++)
            {
                Console.WriteLine($"Expression: {expressions[j]}");

                foreach (var context in listDictResults)
                {
                    foreach (var item in context)
                    {
                        Console.WriteLine($"    {String.Join(",", item.Key.Select(x => "[" + x + "]"))} ==> {item.Value}");
                    }
                }
            }

            ResultSet resultSet = new();
            resultSet.SetColumnNames(outputNames);

            if (groupByExpressions == null || groupByExpressions.Count == 0)
            {
                // with no group by, we should have exactly one row with
                // the number of columns equal to the number of aggregaton expressions
                ExpressionOperand[] outputRow = new ExpressionOperand[expressions.Count];
                for (int j = 0; j < expressions.Count; j++)
                {
                    Console.WriteLine($"Expression: {expressions[j]}");

                    int n = 0;
                    foreach (var kv in listDictResults[j])
                        outputRow[n++] = kv.Value;
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

                    for (int j = 0; j < expressions.Count; j++)
                    {
                        ExpressionOperand result =  expressions[j].Evaluate(new RowsetValueAccessor(rs, i));

                        if (listDictResults[j].ContainsKey(groupByKey))
                            listDictResults[j][groupByKey] = ExpressionOperand.IntegerFromInt(listDictResults[j][groupByKey].AsInteger() + result.AsInteger());
                        else
                            listDictResults[j].Add(groupByKey, result);
                    }
                    // now, evaluate each grouping expression
                    // and apply it to the dictionary at the key
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

