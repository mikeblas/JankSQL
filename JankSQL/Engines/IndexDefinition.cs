namespace JankSQL.Engines
{
    public class IndexDefinition
    {
        private int nextUniquifer = 1;

        internal IndexDefinition(string indexName, bool isUnique, IEnumerable<(string columnName, bool isDescending)> columnInfos, IEngineTable heap)
        {
            this.IndexName = indexName;
            this.IsUnique = isUnique;

            this.ColumnInfos = new List<(string columnName, bool isDescending, int heapColumnIndex)>();
            foreach (var (columnName, isDescending) in columnInfos)
            {
                int idx = heap.ColumnIndex(columnName);
                this.ColumnInfos.Add((columnName, isDescending, idx));
            }
        }

        internal List<(string columnName, bool isDescending, int heapColumnIndex)> ColumnInfos { get; }

        internal bool IsUnique { get; }

        internal string IndexName { get; }

        public int ColumnIndex(string columnName)
        {
            for (int i = 0; i < this.ColumnInfos.Count; i++)
            {
                if (this.ColumnInfos[i].columnName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            return -1;
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
