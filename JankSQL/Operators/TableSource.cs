namespace JankSQL.Operators
{
    using JankSQL.Expressions;

    internal class TableSource : IComponentOutput
    {
        private readonly Engines.IEngineTable source;

        private readonly IEnumerator<Engines.RowWithBookmark> rowEnumerator;
        private readonly string? alias;
        ColumnNameList? allColumnNames;

        private bool enumeratorExhausted;

        internal TableSource(Engines.IEngineTable source)
        {
            this.source = source;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
            this.alias = null;
            this.allColumnNames = null;
        }

        internal TableSource(Engines.IEngineTable source, string? alias)
        {
            this.source = source;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
            this.alias = alias;
            this.allColumnNames = null;
        }


        public void Rewind()
        {
            enumeratorExhausted = false;
            rowEnumerator.Reset();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rs = new (GetAllColumnNames());

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
            {
                rs.MarkEOF();
                return rs;
            }

            return rs;
        }

        protected ColumnNameList GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                allColumnNames = new ColumnNameList(source);
            }

            return allColumnNames;
        }
    }
}


