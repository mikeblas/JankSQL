namespace JankSQL.Engines
{
    using System.Collections.Immutable;

    public class TestTableDefinition
    {
        private readonly Tuple[] rows;
        private readonly FullColumnName[] columnNames;
        private readonly ExpressionOperandType[] columnTypes;
        private readonly FullTableName tableName;
        private readonly List<string>[] indexes;
        private readonly List<string>[] uniqueIndexes;
        private readonly string[] indexNames;
        private readonly string[] uniqueIndexNames;

        internal TestTableDefinition(
            FullTableName tableName,
            IEnumerable<FullColumnName> columnNames,
            IEnumerable<ExpressionOperandType> columnTypes,
            List<Tuple> rows,
            IEnumerable<string> indexNames,
            IEnumerable<List<string>> indexes,
            IEnumerable<string> uniqueIndexNames,
            IEnumerable<List<string>> uniqueIndexes)
        {
            this.tableName = tableName;
            this.rows = rows.ToArray();
            this.columnNames = columnNames.ToArray();
            this.columnTypes = columnTypes.ToArray();
            this.indexes = indexes.ToArray();
            this.uniqueIndexes = uniqueIndexes.ToArray();
            this.indexNames = indexNames.ToArray();
            this.uniqueIndexNames = uniqueIndexNames.ToArray();
        }

        internal int IndexCount
        {
            get { return indexes.Length; }
        }

        internal string[] IndexNames
        {
            get { return indexNames; }
        }

        internal IEnumerable<string>[] IndexColumnNames
        {
            get { return indexes; }
        }

        internal int UniqueIndexCount
        {
            get { return uniqueIndexes.Length; }
        }

        internal string[] UniqueIndexNames
        {
            get { return uniqueIndexNames;  }
        }

        internal IEnumerable<string>[] UniqueIndexColumnNames
        {
            get { return uniqueIndexes;  }
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
