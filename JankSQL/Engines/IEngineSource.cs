namespace JankSQL.Engines
{
    public interface IEngineSource
    {
        int RowCount { get; }
        int ColumnCount { get; }

        string[] Row(int n);

        FullColumnName ColumnName(int n);

        int ColumnIndex(string columnName);

        void Load();
    }
}

