namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    internal class BTreeRowEnumerator : IEnumerator<RowWithBookmark>
    {
        private readonly IEnumerator<KeyValuePair<ExpressionOperand[], ExpressionOperand[]>> treeEnumerator;

        internal BTreeRowEnumerator(BPlusTree<ExpressionOperand[], ExpressionOperand[]> tree)
        {
            treeEnumerator = tree.GetEnumerator();
        }

        public RowWithBookmark Current
        {
            get
            {
                ExpressionOperand[] rowResult = new ExpressionOperand[treeEnumerator.Current.Value.Length + treeEnumerator.Current.Key.Length];

                int n = 0;
                for (int i = 0; i < treeEnumerator.Current.Value.Length; i++)
                    rowResult[n++] = treeEnumerator.Current.Value[i];
                for (int i = 0; i < treeEnumerator.Current.Key.Length; i++)
                    rowResult[n++] = treeEnumerator.Current.Key[i];

                ExpressionOperandBookmark bookmarkResult = new ExpressionOperandBookmark(treeEnumerator.Current.Key);

                return new RowWithBookmark(rowResult, bookmarkResult);
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
