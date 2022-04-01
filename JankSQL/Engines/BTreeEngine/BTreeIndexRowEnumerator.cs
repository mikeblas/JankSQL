namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;
    using JankSQL.Expressions;

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

                // if it's not unique, don't generate the uniquifier
                int keyLen = treeEnumerator.Current.Key.Count - (def.IsUnique ? 0 : 1);
                Tuple keyCopy = Tuple.CreatePartialCopy(keyLen, treeEnumerator.Current.Key);
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
