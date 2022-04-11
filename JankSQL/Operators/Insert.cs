namespace JankSQL.Operators
{
    internal class Insert : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly Engines.IEngineTable engineTable;
        private readonly Dictionary<int, int> targetIndexToInputIndex;

        internal Insert(Engines.IEngineTable destTable, IList<FullColumnName> targetColumns, IComponentOutput input)
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

        public ResultSet? GetRows(int max)
        {
            ResultSet? rsInput = myInput.GetRows(max);
            if (rsInput == null)
                return null;

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
            }

            return rsInput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
