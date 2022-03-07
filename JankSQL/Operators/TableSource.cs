
namespace JankSQL
{

    internal class TableSource : IComponentOutput
    {
        Engines.IEngineSource source;
        int currentRow = 0;

        internal TableSource(Engines.IEngineSource source)
        {
            this.source = source;
            this.source.Load();
        }

        void IComponentOutput.Rewind()
        {
            currentRow = 0;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet rs = new ResultSet();
            List<FullColumnName> columnNames = new List<FullColumnName>();
            for (int n = 0; n < source.ColumnCount; n++)
                columnNames.Add(source.ColumnName(n));
            columnNames.Add(FullColumnName.FromColumnName("bookmark"));
           
            rs.SetColumnNames(columnNames);

            if (currentRow >= source.RowCount)
            {
                return null;
            }

            while (currentRow < source.RowCount && rs.RowCount < max)
            {
                ExpressionOperand[] thisRow = new ExpressionOperand[source.ColumnCount+1];

                for (int i = 0; i < source.ColumnCount; i++)
                {
                    thisRow[i] = source.Row(currentRow)[i];
                }
                thisRow[source.ColumnCount] = ExpressionOperand.IntegerFromInt(currentRow);

                rs.AddRow(thisRow);
                currentRow++;
            }

            return rs;
        }
    }
}


