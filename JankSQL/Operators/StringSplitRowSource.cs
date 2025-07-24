namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class StringSplitRowSource : IOperatorOutput
    {
        private readonly List<FullColumnName> columnNames;
        private readonly string? alias;

        private readonly Expression stringExpression;
        private ExpressionOperand stringValue;
        private readonly Expression separator;
        private ExpressionOperand separatorValue;
        private readonly Expression? ordinal = null;
        private ExpressionOperand? ordinalValue = null;

        private bool needsRewind;
        private bool generateOrdinal;
        private int currentPosition;
        private int nextOrdinal;

        internal StringSplitRowSource(string? alias, Expression stringExpression, Expression separator)
        {
            this.ordinal = null;
            this.stringExpression = stringExpression;
            this.separator = separator;
            this.alias = alias;

            needsRewind = true;
            generateOrdinal = false;

            columnNames = new List<FullColumnName>();
            if (alias == null)
                columnNames.Add(FullColumnName.FromColumnName("value"));
            else
                columnNames.Add(FullColumnName.FromTableColumnName(alias, "value"));
        }

        internal StringSplitRowSource(string? alias, Expression stringExpression, Expression separator, Expression ordinal)
            : this(alias, stringExpression, separator)
        {
            this.ordinal = ordinal;
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            return columnNames.ToArray();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumns, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (ordinal != null)
            {
                ordinalValue = ordinal.Evaluate(null, engine, bindValues);

                if (!ordinalValue.RepresentsNull && ordinalValue.AsInteger() == 1)
                {
                    generateOrdinal = true;
                    if (alias == null)
                        columnNames.Add(FullColumnName.FromColumnName("ordinal"));
                    else
                        columnNames.Add(FullColumnName.FromTableColumnName(alias, "ordinal"));
                }
                else
                {
                    generateOrdinal = false;
                }
            }
            else
            {
                // no ordinal argument, so we don't generate that column
                generateOrdinal = false;
            }

            return BindResult.Success();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (needsRewind)
            {
                stringValue = stringExpression.Evaluate(outerAccessor, engine, bindValues);
                separatorValue = separator.Evaluate(outerAccessor, engine, bindValues);

                currentPosition = 0;
                nextOrdinal = 1;
                needsRewind = false;
            }

            if (stringValue == null || separatorValue == null)
                throw new InternalErrorException("SplitStringRowSource was not rewound before producing rows");

            ResultSet resultSet = new (columnNames);

            if (currentPosition == -1 || currentPosition >= stringValue.AsString().Length)
            {
                resultSet.MarkEOF();
                return resultSet;
            }

            //REVIEW: t isn't used here, so paging isn't working as expected
            int t = 0;
            while (t < max && currentPosition != -1 && currentPosition < stringValue.AsString().Length)
            {
                //REVIEW: t isn't used, so this isn't paging correctly
                Tuple generatedValues = Tuple.CreateEmpty(columnNames.Count);

                string match;
                int temp = stringValue.AsString().IndexOf(separatorValue.AsString(), currentPosition);
                if (temp == -1)
                    match = stringValue.AsString().Substring(currentPosition);
                else
                    match = stringValue.AsString().Substring(currentPosition, temp  - currentPosition);


                // YOU WERE HERE AT LANDING

                generatedValues[0] = ExpressionOperand.VARCHARFromString(match);
                if (generateOrdinal)
                    generatedValues[1] = ExpressionOperand.IntegerFromInt(nextOrdinal);
                nextOrdinal++;

                resultSet.AddRow(generatedValues);

                // move to next
                if (temp != -1)
                    currentPosition = temp + 1;
                else
                    currentPosition = temp;
            }

            return resultSet;
        }

        public void Rewind()
        {
            needsRewind = true;
        }
    }
}
