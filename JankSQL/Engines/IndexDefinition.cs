namespace JankSQL.Engines
{
    public class IndexDefinition
    {
        private readonly string indexName;
        private readonly bool isUnique;
        private readonly List<(string columnName, bool isDescending, int heapColumnIndex)> columnInfos;
        private int nextUniquifer = 1;

        internal IndexDefinition(string indexName, bool isUnique, List<(string columnName, bool isDescending)> columnInfos, IEngineTable heap)
        {
            this.indexName = indexName;
            this.isUnique = isUnique;

            this.columnInfos = new List<(string columnName, bool isDescending, int heapColumnIndex)>();
            foreach (var columnInfo in columnInfos)
            {
                int idx = heap.ColumnIndex(columnInfo.columnName);
                this.columnInfos.Add((columnInfo.columnName, columnInfo.isDescending, idx));
            }
        }

        public int ColumnIndex(string columnName)
        {
            for (int i = 0; i < this.columnInfos.Count; i++)
            {
                if (this.columnInfos[i].columnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        internal List<(string columnName, bool isDescending, int heapColumnIndex)> ColumnInfos
        {
            get { return columnInfos; }
        }

        internal bool IsUnique
        {
            get { return isUnique; }
        }

        internal string IndexName
        {
            get { return indexName; }
        }

        internal int AdvanceNextUniquifier()
        {
            return nextUniquifer++;
        }

        internal Tuple IndexKeyFromHeapRow(Tuple heapRow, BTreeTable heap)
        {
            Tuple indexKey;
            if (IsUnique)
            {
                indexKey = Tuple.CreateEmpty(ColumnInfos.Count);

                for (int i = 0; i < ColumnInfos.Count; i++)
                    indexKey.Values[i] = heapRow[ColumnInfos[i].heapColumnIndex];
            }
            else
            {
                // for a non-unique index, the key is as given plus a uniquifier integer
                indexKey = Tuple.CreateEmpty(ColumnInfos.Count + 1);
                for (int i = 0; i < ColumnInfos.Count; i++)
                    indexKey.Values[i] = heapRow[ColumnInfos[i].heapColumnIndex];

                indexKey[ColumnInfos.Count] = ExpressionOperand.IntegerFromInt(AdvanceNextUniquifier());
            }

            return indexKey;
        }

    }
}
