namespace JankSQL.Engines
{
    public interface IEngineTable
    {
        //=== metadata
        // get the number of rows
        int RowCount { get; }

        // get the number of columns
        int ColumnCount { get; }

        // get a column name given an index
        FullColumnName ColumnName(int n);

        // get the index of a column name
        int ColumnIndex(string columnName);

        //=== data access
        // get all the columns in the given row
        ExpressionOperand[] Row(int n);


        //=== DML
        // truncate this table
        void TruncateTable();

        void InsertRow(ExpressionOperand[] row);

        int DeleteRows(List<int> rowIndexesToDelete);

    }
}

