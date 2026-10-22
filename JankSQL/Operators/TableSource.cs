namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class TableSource : IOperatorOutput
    {
        private readonly Engines.IEngineTable source;

        private readonly IEnumerator<Engines.RowWithBookmark> rowEnumerator;
        private readonly string? alias;

        private bool enumeratorExhausted;

        private List<FullColumnName>? allColumnNames;

        internal TableSource(Engines.IEngineTable source)
        {
            this.source = source;
            allColumnNames = null;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
            this.alias = null;
        }

        internal TableSource(Engines.IEngineTable source, string? alias)
        {
            this.source = source;
            allColumnNames = null;
            rowEnumerator = this.source.GetEnumerator();
            enumeratorExhausted = false;
            this.alias = alias;
        }


        public void Rewind()
        {
            enumeratorExhausted = false;
            rowEnumerator.Reset();
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            FullColumnName[] fcn = GetAllColumnNames().ToArray();
            return fcn;
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            return BindResult.Success();
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rs = new (GetAllColumnNames());

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

        protected List<FullColumnName> GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                allColumnNames = new ();
                for (int n = 0; n < source.ColumnCount; n++)
                {
                    FullColumnName fcn = source.ColumnName(n);
                    if (alias != null)
                        fcn = fcn.ApplyTableAlias(alias);
                    allColumnNames.Add(fcn);
                }

                allColumnNames.Add(FullColumnName.FromColumnName("bookmark_key"));
            }

            return allColumnNames;
        }
    }
}
