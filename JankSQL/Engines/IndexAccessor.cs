namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    public class IndexAccessor : /* IEnumerable, */ IEnumerable<RowWithBookmark>
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
    }
}

