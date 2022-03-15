
namespace JankSQL
{

    internal class TableSource : IComponentOutput
    {
        Engines.IEngineTable source;
        // int currentRow = 0;
        IEnumerator<ExpressionOperand[]> rowEnumerator;
        bool enumeratorExhausted;

        internal TableSource(Engines.IEngineTable source)
        {
            this.source = source;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
        }

        void IComponentOutput.Rewind()
        {
            // currentRow = 0;
            enumeratorExhausted = false;
            rowEnumerator.Reset();
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet rs = new ResultSet();
            List<FullColumnName> columnNames = new List<FullColumnName>();
            for (int n = 0; n < source.ColumnCount; n++)
                columnNames.Add(source.ColumnName(n));
            // columnNames.Add(FullColumnName.FromColumnName("bookmark_key"));
           
            rs.SetColumnNames(columnNames);

            if (enumeratorExhausted)
                return null;

            while (!enumeratorExhausted && rs.RowCount < max)
            {
                enumeratorExhausted = !rowEnumerator.MoveNext();
                if (enumeratorExhausted)
                    break;

                ExpressionOperand[] thisRow = new ExpressionOperand[source.ColumnCount];

                for (int i = 0; i < source.ColumnCount; i++)
                {
                    thisRow[i] = rowEnumerator.Current[i];
                }
                // thisRow[source.ColumnCount] = ExpressionOperand.IntegerFromInt(currentRow);

                rs.AddRow(thisRow);
            }

            /*
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
            */

            return rs;
        }
    }
}


