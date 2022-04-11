namespace JankSQL.Operators
{
    internal class ConstantRowSource : IComponentOutput
    {
        private readonly List<List<Expression>> columnValues;
        private readonly IList<FullColumnName> columnNames;

        private int currentRow = 0;

        internal ConstantRowSource(IList<FullColumnName> columnNames, List<List<Expression>> columnValues)
        {
            this.currentRow = 0;
            this.columnValues = columnValues;
            this.columnNames = columnNames;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet resultSet = new (columnNames);

            if (currentRow >= columnValues.Count)
            {
                resultSet.MarkEOF();
                return resultSet;
            }

            int t = 0;
            while (t < max && currentRow < columnValues.Count)
            {
                Tuple generatedValues = Tuple.CreateEmpty(columnValues[0].Count);

                for (int i = 0; i < columnValues[currentRow].Count; i++)
                    generatedValues[i] = columnValues[currentRow][i].Evaluate(null);

                resultSet.AddRow(generatedValues);
                currentRow++;
            }

            return resultSet;
        }

        public void Rewind()
        {
            currentRow = 0;
        }
    }
}
