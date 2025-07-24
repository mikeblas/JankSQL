namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Insert : IOperatorOutput
    {
        private readonly IOperatorOutput myInput;
        private readonly Engines.IEngineTable engineTable;
        private readonly Dictionary<int, int> targetIndexToInputIndex;

        private int rowsAffected;

        internal Insert(Engines.IEngineTable destTable, IList<FullColumnName> targetColumns, IOperatorOutput input)
        {
            myInput = input;
            engineTable = destTable;

            targetIndexToInputIndex = new Dictionary<int, int>();

            for (int i = 0; i < targetColumns.Count; i++)
            {
                int targetIndex = destTable.ColumnIndex(targetColumns[i].ColumnNameOnly());
                if (targetIndex == -1)
                    throw new ExecutionException($"column {targetColumns[i]} not found in target");
                targetIndexToInputIndex[targetIndex] = i;
            }
        }

        internal int RowsAffected
        {
            get { return rowsAffected; }
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            return myInput.GetOutputColumnNames();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            return BindResult.Success();
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rsInput = myInput.GetRows(engine, outerAccessor, max, bindValues);
            if (rsInput.IsEOF)
            {
                ResultSet eof = new (new List<FullColumnName>());
                eof.MarkEOF();
                return eof;
            }

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                Tuple sourceRow = rsInput.Row(i);
                Tuple targetRow = Tuple.CreateEmpty(engineTable.ColumnCount);

                for (int j = 0; j < engineTable.ColumnCount; j++)
                {
                    if (targetIndexToInputIndex.TryGetValue(j, out int inputIndex))
                        targetRow[j] = sourceRow[inputIndex];
                    else
                        targetRow[j] = ExpressionOperand.NullLiteral();
                }

                engineTable.InsertRow(targetRow);
                rowsAffected += 1;
            }

            return rsInput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
