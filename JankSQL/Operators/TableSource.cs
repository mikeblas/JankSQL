
namespace JankSQL
{

    internal class TableSource : IComponentOutput
    {
        readonly Engines.IEngineTable source;

        // int currentRow = 0;
        readonly IEnumerator<Engines.RowWithBookmark> rowEnumerator;
        bool enumeratorExhausted;

        internal TableSource(Engines.IEngineTable source)
        {
            this.source = source;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
        }

        public void Rewind()
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
            columnNames.Add(FullColumnName.FromColumnName("bookmark_key"));
           
            rs.SetColumnNames(columnNames);

            if (enumeratorExhausted)
                return null;

            while (!enumeratorExhausted && rs.RowCount < max)
            {
                enumeratorExhausted = !rowEnumerator.MoveNext();
                if (enumeratorExhausted)
                    break;

                ExpressionOperand[] thisRow = new ExpressionOperand[source.ColumnCount + 1];

                Array.Copy(rowEnumerator.Current.RowData, 0, thisRow, 0, source.ColumnCount);

                thisRow[source.ColumnCount] = rowEnumerator.Current.Bookmark;

                rs.AddRow(thisRow);
            }

            return rs;
        }
    }
}


