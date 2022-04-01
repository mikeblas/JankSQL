namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    public class IndexAccessor : IEnumerable, IEnumerable<RowWithBookmark>
    {
        private readonly IndexDefinition def;
        private readonly BPlusTree<Tuple, Tuple> indexTree;

        internal IndexAccessor(IndexDefinition indexDefinition, BPlusTree<Tuple, Tuple> index)
        {
            def = indexDefinition;
            indexTree = index;
        }

        public IndexDefinition IndexDefinition
        {
            get { return def; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new BTreeIndexRowEnumerator(indexTree, def);
        }

        IEnumerator<RowWithBookmark> IEnumerable<RowWithBookmark>.GetEnumerator()
        {
            return new BTreeIndexRowEnumerator(indexTree, def);
        }

        internal void Dump()
        {
            Console.WriteLine($"=====");
            Console.WriteLine($"index {def.IndexName}");
            string s = string.Join(",", def.ColumnInfos.Select(x => $"[{x.columnName}, {(x.isDescending ? "DESC" : "ASC")}]"));
            Console.WriteLine($"   {s}");

            foreach (var r in this)
                Console.WriteLine($"   {r.RowData} ==> {r.Bookmark}");
        }
    }
}

