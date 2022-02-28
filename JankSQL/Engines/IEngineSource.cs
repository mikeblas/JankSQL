namespace JankSQL.Engines
{
    public interface IEngineSource
    {
        // initialize (load) this table into memory
        void Load();

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

    }
}

