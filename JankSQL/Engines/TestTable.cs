namespace JankSQL.Engines
{
    public class TestTable
    {
        private readonly List<Tuple> rows;
        private readonly List<FullColumnName> columnNames;
        private readonly List<ExpressionOperandType> columnTypes;
        private readonly FullTableName tableName;

        internal TestTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes, List<Tuple> rows)
        {
            this.tableName = tableName;
            this.rows = rows;
            this.columnNames = columnNames;
            this.columnTypes = columnTypes;
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        internal List<ExpressionOperandType> ColumnTypes
        {
            get { return columnTypes; }
        }

        internal List<FullColumnName> ColumnNames
        {
            get { return columnNames; }
        }

        internal List<Tuple> Rows
        {
            get { return rows; }
        }
    }
}
