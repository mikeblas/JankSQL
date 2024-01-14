namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    public class IndexAccessor : IEnumerable, IEnumerable<RowWithBookmark>
    {
        private readonly BPlusTree<Tuple, Tuple> indexTree;

        internal IndexAccessor(IndexDefinition indexDefinition, BPlusTree<Tuple, Tuple> index)
        {
            IndexDefinition = indexDefinition;
            indexTree = index;
        }

        public IndexDefinition IndexDefinition { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new BTreeIndexRowEnumerator(indexTree, IndexDefinition);
        }

        IEnumerator<RowWithBookmark> IEnumerable<RowWithBookmark>.GetEnumerator()
        {
            return new BTreeIndexRowEnumerator(indexTree, IndexDefinition);
        }

        internal void Dump()
        {
            Console.WriteLine("=====");
            Console.WriteLine("index {IndexDefinition.IndexName}");
            string s = string.Join(",", IndexDefinition.ColumnInfos.Select(x => $"[{x.columnName}, {(x.isDescending ? "DESC" : "ASC")}]"));
            Console.WriteLine($"   {s}");

            foreach (var r in this)
                Console.WriteLine($"   {r.RowData} ==> {r.Bookmark}");
        }
    }
}