namespace JankSQL.Engines
{
    using System.Collections.Immutable;

    using JankSQL.Expressions;

    public class TestTable
    {
        private readonly Tuple[] rows;
        private readonly FullColumnName[] columnNames;
        private readonly ExpressionOperandType[] columnTypes;
        private readonly FullTableName tableName;

        internal TestTable(FullTableName tableName, IList<FullColumnName> columnNames, IList<ExpressionOperandType> columnTypes, List<Tuple> rows)
        {
            this.tableName = tableName;
            this.rows = rows.ToArray();
            this.columnNames = columnNames.ToArray();
            this.columnTypes = columnTypes.ToArray();
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        internal IImmutableList<ExpressionOperandType> ColumnTypes
        {
            get { return columnTypes.ToImmutableList(); }
        }

        internal IImmutableList<FullColumnName> ColumnNames
        {
            get { return columnNames.ToImmutableList(); }
        }

        internal IImmutableList<Tuple> Rows
        {
            get { return rows.ToImmutableList(); }
        }
    }
}
