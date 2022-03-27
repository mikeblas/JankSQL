namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    internal class BTreeIndexRowEnumerator : IEnumerator<RowWithBookmark>
    {
        private readonly IEnumerator<KeyValuePair<Tuple, Tuple>> treeEnumerator;
        private readonly IndexDefinition def;

        internal BTreeIndexRowEnumerator(BPlusTree<Tuple, Tuple> tree, IndexDefinition indexDefinition)
        {
            treeEnumerator = tree.GetEnumerator();
            def = indexDefinition;
        }

        public RowWithBookmark Current
        {
            get
            {
                ExpressionOperandBookmark bookmarkResult = new (treeEnumerator.Current.Value);

                Tuple keyCopy = Tuple.CreatePartialCopy(treeEnumerator.Current.Key.Count - 1, treeEnumerator.Current.Key);
                return new RowWithBookmark(keyCopy, bookmarkResult);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            treeEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return treeEnumerator.MoveNext();
        }

        public void Reset()
        {
            treeEnumerator.Reset();
        }
    }
}
