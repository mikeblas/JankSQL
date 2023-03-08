namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    //REVIEW: maybe this should be an operator and not an Engine
    public class DualSource : IEngineTable
    {
        public int ColumnCount => 0;

        public int ColumnIndex(string columnName)
        {
            return -1;
        }

        public FullColumnName ColumnName(int n)
        {
            throw new NotImplementedException();
        }

        public void TruncateTable()
        {
            throw new NotImplementedException();
        }

        public void InsertRow(Tuple row)
        {
            throw new NotImplementedException();
        }

        public int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<RowWithBookmark> GetEnumerator()
        {
            List<RowWithBookmark> xl = new ();

            Tuple x = Tuple.CreateEmpty();
            ExpressionOperandBookmark bm = new ExpressionOperandBookmark(Tuple.CreateEmpty());

            xl.Add(new RowWithBookmark(x, bm));

            return xl.GetEnumerator();
        }

        public IndexAccessor Index(string indexName)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}

