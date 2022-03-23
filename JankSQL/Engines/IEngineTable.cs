namespace JankSQL.Engines
{
    public interface IEngineTable
    {
        // === metadata
        // get the number of columns
        int ColumnCount { get; }

        // get a column name given an index
        FullColumnName ColumnName(int n);

        // get the index of a column name
        int ColumnIndex(string columnName);

        // === data access
        // iterate over the available rows
        IEnumerator<RowWithBookmark> GetEnumerator();

        // === DML
        // truncate this table
        void TruncateTable();

        void InsertRow(Tuple row);

        int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete);
    }
}
