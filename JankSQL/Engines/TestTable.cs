namespace JankSQL.Engines
{
    public class TestTableBuilder
    {
        private readonly List<FullColumnName> columnNames = new ();
        private List<object[]>? rows;
        private List<ExpressionOperandType>? columnTypes;

        private FullTableName? tableName;

        public static TestTableBuilder NewBuilder()
        {
            return new TestTableBuilder();
        }

        public TestTableBuilder WithTableName(string tableName)
        {
            this.tableName = FullTableName.FromTableName(tableName);
            return this;
        }

        public TestTableBuilder WithColumnTypes(ExpressionOperandType[] columnTypes)
        {
            this.columnTypes = columnTypes.ToList();
            return this;
        }

        public TestTableBuilder WithColumnNames(string[] columnNames)
        {
            foreach (string name in columnNames)
                this.columnNames.Add(FullColumnName.FromColumnName(name));
            return this;
        }

        public TestTableBuilder WithRow(object[] row)
        {
            if (rows == null)
                rows = new List<object[]>();
            rows.Add(row);
            return this;
        }

        public TestTable Build()
        {
            List<ExpressionOperand[]>? convertedRows = new List<ExpressionOperand[]>();

            if (columnTypes == null)
                throw new InvalidOperationException("no column types provided");
            if (tableName == null)
                throw new InvalidOperationException("no table name provided");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    ExpressionOperand[] convertedRow = new ExpressionOperand[row.Length];

                    if (columnTypes.Count != row.Length)
                        throw new ArgumentException($"found a row with {row.Length} values, but {columnTypes.Count} column types");

                    for (int i = 0; i < row.Length; i++)
                    {
                        var op = ExpressionOperand.FromObjectAndType(row[i], columnTypes[i]);
                        convertedRow[i] = op;
                    }

                    convertedRows.Add(convertedRow);
                }
            }

            TestTable table = new TestTable(tableName, columnNames, columnTypes, convertedRows);
            return table;
        }
    }

    public class TestTable
    {
        private readonly List<ExpressionOperand[]> rows;
        private readonly List<FullColumnName> columnNames;
        private readonly List<ExpressionOperandType> columnTypes;
        private readonly FullTableName tableName;

        internal FullTableName TableName { get { return tableName; } }

        internal List<ExpressionOperandType> ColumnTypes { get { return columnTypes; } }

        internal List<FullColumnName> ColumnNames { get { return columnNames; } }

        internal List<ExpressionOperand[]> Rows { get { return rows; } }

        internal TestTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes, List<ExpressionOperand[]> rows)
        {
            this.tableName = tableName;
            this.rows = rows;
            this.columnNames = columnNames;
            this.columnTypes = columnTypes;
            this.rows = rows;
        }

    }
}
