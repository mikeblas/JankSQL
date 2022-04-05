namespace JankSQL.Operators
{
    internal class TableSource : IComponentOutput
    {
        private readonly Engines.IEngineTable source;

        private readonly IEnumerator<Engines.RowWithBookmark> rowEnumerator;
        private bool enumeratorExhausted;

        List<FullColumnName>? columnNames;

        internal TableSource(Engines.IEngineTable source)
        {
            this.source = source;
            columnNames = null;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
        }

        public void Rewind()
        {
            enumeratorExhausted = false;
            rowEnumerator.Reset();
        }

        protected void BuildColumnNames()
        {
            columnNames = new ();
            for (int n = 0; n < source.ColumnCount; n++)
                columnNames.Add(source.ColumnName(n));
            columnNames.Add(FullColumnName.FromColumnName("bookmark_key"));
        }

        public ResultSet? GetRows(int max)
        {
            if (columnNames == null)
                BuildColumnNames();

            ResultSet rs = new (columnNames);

            if (enumeratorExhausted)
                return null;

            while (!enumeratorExhausted && rs.RowCount < max)
            {
                enumeratorExhausted = !rowEnumerator.MoveNext();
                if (enumeratorExhausted)
                    break;

                // create a new tuple with all the columns from the source
                // and append the bookmark to it
                Tuple thisRow = Tuple.CreateSuperCopy(source.ColumnCount + 1, rowEnumerator.Current.RowData);
                thisRow[source.ColumnCount] = rowEnumerator.Current.Bookmark;

                rs.AddRow(thisRow);
            }

            if (enumeratorExhausted && rs.RowCount == 0)
                return null;

            return rs;
        }
    }
}


