namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    public class TestTableBuilder
    {
        private readonly List<FullColumnName> columnNames = new ();
        private List<object?[]>? rows;
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

        public TestTableBuilder WithRow(object?[] row)
        {
            (rows ??= new List<object?[]>()).Add(row);
            return this;
        }

        public TestTable Build()
        {
            var convertedRows = new List<Tuple>();

            if (columnTypes == null)
                throw new InvalidOperationException("no column types provided");
            if (tableName == null)
                throw new InvalidOperationException("no table name provided");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    Tuple convertedRow = Tuple.CreateEmpty(row.Length);

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

            TestTable table = new (tableName, columnNames, columnTypes, convertedRows);
            return table;
        }
    }
}
